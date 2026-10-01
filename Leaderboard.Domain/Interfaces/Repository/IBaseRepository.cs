using Leaderboard.Domain.Interfaces.Database;

namespace Leaderboard.Domain.Interfaces.Repository;

public interface IBaseRepository<TEntity> : IStateSaveChanges
{
    IQueryable<TEntity> GetAll();

    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task CreateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    TEntity Update(TEntity entity);

    TEntity Remove(TEntity entity);
}