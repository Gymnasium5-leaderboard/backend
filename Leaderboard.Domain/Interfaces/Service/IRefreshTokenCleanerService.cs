namespace Leaderboard.Domain.Interfaces.Service;

public interface IRefreshTokenCleanerService
{
    /// <summary>
    ///     Removes expired refresh tokens. Returns the number of removed tokens.
    /// </summary>
    Task<int> RemoveExpiredAsync(CancellationToken cancellationToken = default);
}