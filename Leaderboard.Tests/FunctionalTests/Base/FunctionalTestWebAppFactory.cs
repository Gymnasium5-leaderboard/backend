using Leaderboard.BackgroundJobs.Jobs;
using Leaderboard.Tests.FunctionalTests.Configurations;
using Leaderboard.Tests.FunctionalTests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Base;

public class FunctionalTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _leaderboardPostgreSql = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("leaderboard-db")
        .WithUsername("postgres")
        .WithPassword("root")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:7").Build();

    public async Task InitializeAsync()
    {
        await _leaderboardPostgreSql.StartAsync();
        await _redisContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _leaderboardPostgreSql.StopAsync();
        await _redisContainer.StopAsync();
    }

    /// <summary>
    ///     Removes all keys, so a test does not read data cached by the previous one.
    /// </summary>
    public async Task FlushCacheAsync()
    {
        await using var multiplexer =
            await ConnectionMultiplexer.ConnectAsync($"{_redisContainer.GetConnectionString()},allowAdmin=true");
        await multiplexer.GetServers().Single().FlushDatabaseAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Program.cs reads these while the app is being built, so they go in as host settings,
        // not through ConfigureAppConfiguration
        builder.UseSetting("ConnectionStrings:PostgresSQL", _leaderboardPostgreSql.GetConnectionString());
        builder.UseSetting("JwtSettings:Issuer", TokenHelper.Issuer);
        builder.UseSetting("JwtSettings:Audience", TokenHelper.Audience);
        builder.UseSetting("JwtSettings:SigningKey", TokenHelper.SigningKey);
        builder.UseSetting("RedisSettings:Host", _redisContainer.Hostname);
        builder.UseSetting("RedisSettings:Port",
            _redisContainer.GetMappedPublicPort(RedisBuilder.RedisPort).ToString());

        builder.ConfigureTestServices(services =>
        {
            // The job removes expired tokens on startup, at the same time as a test resets the database
            services.Remove(services.Single(x => x.ImplementationType == typeof(RefreshTokenCleanupService)));

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateAsyncScope();
            scope.PrepPopulation();
        });
    }
}