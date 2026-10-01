using Leaderboard.DAL.Locks;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Database;
using Leaderboard.Domain.Interfaces.Repository;

namespace Leaderboard.DAL.Repositories;

public class UnitOfWork(
    ApplicationDbContext context,
    IBaseRepository<LeaderboardOwner> ownerRepository,
    IBaseRepository<Student> studentRepository,
    IBaseRepository<ScoreTransaction> scoreTransactionRepository,
    ILockAcquirer lockAcquirer) : IUnitOfWork
{
    public IBaseRepository<LeaderboardOwner> Owners { get; set; } = ownerRepository;
    public IBaseRepository<Student> Students { get; set; } = studentRepository;
    public IBaseRepository<ScoreTransaction> ScoreTransactions { get; set; } = scoreTransactionRepository;

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        return new DbContextTransaction(transaction);
    }

    public Task AcquireLockAsync(IReadOnlyCollection<long> keys, CancellationToken cancellationToken = default)
    {
        return lockAcquirer.AcquireAsync(keys, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}