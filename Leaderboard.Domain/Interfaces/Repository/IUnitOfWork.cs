using Leaderboard.Domain.Interfaces.Database;

namespace Leaderboard.Domain.Interfaces.Repository;

public interface IUnitOfWork : IStateSaveChanges
{
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
