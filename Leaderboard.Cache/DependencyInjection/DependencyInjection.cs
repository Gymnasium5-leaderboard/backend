using Leaderboard.Cache.Interfaces;
using Leaderboard.Cache.Notifiers;
using Leaderboard.Cache.Providers;
using Leaderboard.Cache.Repositories;
using Leaderboard.Cache.Settings;
using Leaderboard.Domain.Interfaces.Notifier;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using StackExchange.Redis;

namespace Leaderboard.Cache.DependencyInjection;

public static class DependencyInjection
{
    public static void AddCache(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var redisSettings = provider.GetRequiredService<IOptions<RedisSettings>>().Value;
            var configuration = new ConfigurationOptions
            {
                EndPoints = { { redisSettings.Host, redisSettings.Port } },
                Password = redisSettings.Password,
                // The API keeps working on PostgreSQL while Redis is down and reconnects in the background
                AbortOnConnectFail = false,
                BacklogPolicy = BacklogPolicy.FailFast,
                ConnectTimeout = 1000,
                AsyncTimeout = 1000,
                SyncTimeout = 1000
            };

            var multiplexer = ConnectionMultiplexer.Connect(configuration);
            multiplexer.LogConnectionState(provider.GetRequiredService<ILogger>());

            return multiplexer;
        });

        services.AddScoped<IDatabase>(provider =>
        {
            var multiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
            return multiplexer.GetDatabase();
        });

        services.InitProviders();
        services.InitRepositories();
        services.InitNotifiers();
    }

    /// <summary>
    ///     Logs once when Redis becomes unavailable and once when it is back, instead of an error on every request.
    /// </summary>
    private static void LogConnectionState(this IConnectionMultiplexer multiplexer, ILogger logger)
    {
        var isDown = 0;

        if (!multiplexer.IsConnected)
        {
            isDown = 1;
            logger.Error("Redis at {EndPoints} is unavailable at startup, the cache is bypassed until it connects",
                multiplexer.GetEndPoints());
        }

        multiplexer.ConnectionFailed += (_, e) =>
        {
            if (Interlocked.Exchange(ref isDown, 1) == 0)
                logger.Error(e.Exception,
                    "Redis connection to {EndPoint} failed ({FailureType}), the cache is bypassed",
                    e.EndPoint, e.FailureType);
        };

        multiplexer.ConnectionRestored += (_, e) =>
        {
            if (Interlocked.Exchange(ref isDown, 0) == 1)
                logger.Information("Redis connection to {EndPoint} restored", e.EndPoint);
        };
    }

    private static void InitProviders(this IServiceCollection services)
    {
        services.AddScoped<ICacheProvider, RedisCacheProvider>();
    }

    private static void InitNotifiers(this IServiceCollection services)
    {
        // One instance holds the subscribers and the Redis subscription, which is made on startup
        services.AddSingleton<ILeaderboardNotifier, LeaderboardNotifier>();
        services.AddHostedService(provider =>
            (LeaderboardNotifier)provider.GetRequiredService<ILeaderboardNotifier>());
    }

    private static void InitRepositories(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<ClassCacheRepository>()
            .AddClasses(c => c.InExactNamespaceOf<ClassCacheRepository>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }
}