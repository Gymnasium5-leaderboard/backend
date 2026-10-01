using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Database;

namespace Leaderboard.Domain.Interfaces.Repository;

public interface IUnitOfWork : IStateSaveChanges
{
    public IBaseRepository<LeaderboardOwner> Owners { get; set; }
    public IBaseRepository<Student> Students { get; set; }
    public IBaseRepository<ScoreTransaction> ScoreTransactions { get; set; }
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task AcquireLockAsync(IReadOnlyCollection<long> keys, CancellationToken cancellationToken = default);
}