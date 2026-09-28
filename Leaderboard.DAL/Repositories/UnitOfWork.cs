using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Database;
using Leaderboard.Domain.Interfaces.Repository;

namespace Leaderboard.DAL.Repositories;

public class UnitOfWork(ApplicationDbContext context, IBaseRepository<LeaderboardOwner> ownerRepository) : IUnitOfWork
{
    public IBaseRepository<LeaderboardOwner> OwnerRepository { get; set; } = ownerRepository;

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        return new DbContextTransaction(transaction);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}