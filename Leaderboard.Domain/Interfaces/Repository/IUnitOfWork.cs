using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Database;

namespace Leaderboard.Domain.Interfaces.Repository;

public interface IUnitOfWork : IStateSaveChanges
{
    public IBaseRepository<LeaderboardOwner> OwnerRepository { get; set; }
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}