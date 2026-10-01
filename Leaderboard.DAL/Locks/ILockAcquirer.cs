namespace Leaderboard.DAL.Locks;

/// <summary>
///     Database-specific exclusive locks held until the current transaction ends.
/// </summary>
public interface ILockAcquirer
{
    Task AcquireAsync(IReadOnlyCollection<long> keys, CancellationToken cancellationToken = default);
}