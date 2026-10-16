using Leaderboard.Domain.Dtos.Category;

namespace Leaderboard.Domain.Interfaces.Repository.Cache;

/// <summary>
///     All categories, archived ones too, are stored as one list. Cache errors are logged, not thrown: a failed read
///     is a miss.
/// </summary>
public interface ICategoryCacheRepository
{
    Task<IReadOnlyCollection<CategoryDto>?> GetAllAsync(CancellationToken cancellationToken = default);

    Task SetAllAsync(IReadOnlyCollection<CategoryDto> categories);

    Task RemoveAllAsync();
}
