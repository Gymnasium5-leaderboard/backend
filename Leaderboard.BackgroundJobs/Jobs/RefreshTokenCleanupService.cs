using Leaderboard.Domain.Interfaces.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Leaderboard.BackgroundJobs.Jobs;

/// <summary>
///     Removes expired refresh tokens on startup and then once a day.
/// </summary>
public class RefreshTokenCleanupService(IServiceScopeFactory scopeFactory, TimeProvider timeProvider, ILogger logger)
    : BackgroundService
{
    public static readonly TimeSpan Interval = TimeSpan.FromDays(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval, timeProvider);

        // Cancellation ends the task as canceled, the host does not treat it as a failure
        do
        {
            await CleanUpAsync(stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CleanUpAsync(CancellationToken stoppingToken)
    {
        // A failed run must not stop the host: the next one runs on the next tick
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var cleaner = scope.ServiceProvider.GetRequiredService<IRefreshTokenCleanerService>();
            var removed = await cleaner.RemoveExpiredAsync(stoppingToken);
            logger.Information("Removed {Count} expired refresh tokens", removed);
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            logger.Error(ex, "Failed to remove expired refresh tokens");
        }
    }
}