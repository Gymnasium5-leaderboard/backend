using Leaderboard.Tests.FunctionalTests.Configurations;
using Leaderboard.Tests.FunctionalTests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Leaderboard.Tests.FunctionalTests.Base;

public class FunctionalTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _leaderboardPostgreSql = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("leaderboard-db")
        .WithUsername("postgres")
        .WithPassword("root")
        .Build();

    public async Task InitializeAsync()
    {
        await _leaderboardPostgreSql.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _leaderboardPostgreSql.StopAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Program.cs reads these while the app is being built, so they go in as host settings,
        // not through ConfigureAppConfiguration
        builder.UseSetting("ConnectionStrings:PostgresSQL", _leaderboardPostgreSql.GetConnectionString());
        builder.UseSetting("JwtSettings:Issuer", TokenHelper.Issuer);
        builder.UseSetting("JwtSettings:Audience", TokenHelper.Audience);
        builder.UseSetting("JwtSettings:SigningKey", TokenHelper.SigningKey);

        builder.ConfigureTestServices(services =>
        {
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateAsyncScope();
            scope.PrepPopulation();
        });
    }
}