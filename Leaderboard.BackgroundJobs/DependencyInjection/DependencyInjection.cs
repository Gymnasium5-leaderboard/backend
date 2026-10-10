using Leaderboard.BackgroundJobs.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Leaderboard.BackgroundJobs.DependencyInjection;

public static class DependencyInjection
{
    public static void AddBackgroundJobs(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddHostedService<RefreshTokenCleanupService>();
    }
}