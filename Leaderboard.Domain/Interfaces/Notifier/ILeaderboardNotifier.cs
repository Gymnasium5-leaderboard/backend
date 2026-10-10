namespace Leaderboard.Domain.Interfaces.Notifier;

/// <summary>
///     Tells clients that the leaderboards changed. The event carries no leaderboard: clients load the ones they show
///     again through the usual endpoints, which are cached.
/// </summary>
public interface ILeaderboardNotifier
{
    /// <summary>
    ///     Sends the change to subscribers of every instance of the API. Call it after the cached leaderboards are
    ///     removed, otherwise clients load the old ones. Errors are logged, not thrown.
    /// </summary>
    Task NotifyChangedAsync();

    /// <summary>
    ///     Yields the time of each change until the token is canceled. Changes that come before the subscriber reads
    ///     the previous one are merged into one.
    /// </summary>
    IAsyncEnumerable<DateTime> SubscribeAsync(CancellationToken cancellationToken);
}