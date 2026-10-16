using Leaderboard.Domain.Dtos.Category;
using Leaderboard.Domain.Results;

namespace Leaderboard.Domain.Interfaces.Service;

/// <summary>
///     Categories of score changes. An archived category is not counted anywhere except the history.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    ///     Categories ordered by id, without archived ones unless <paramref name="includeArchived" /> is set.
    /// </summary>
    Task<CollectionResult<CategoryDto>>
        GetAllAsync(bool includeArchived, CancellationToken cancellationToken = default);

    /// <summary>
    ///     A category by id, archived ones too.
    /// </summary>
    Task<BaseResult<CategoryDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<BaseResult<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Renames a category, archived ones too.
    /// </summary>
    Task<BaseResult<CategoryDto>> UpdateAsync(long id, UpdateCategoryDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Archives a category. An archived category stays archived.
    /// </summary>
    Task<BaseResult<CategoryDto>> ArchiveAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns a category from the archive. A category that is not archived stays as it is.
    /// </summary>
    Task<BaseResult<CategoryDto>> RestoreAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a category that has no transactions in any academic year.
    /// </summary>
    Task<BaseResult> DeleteAsync(long id, CancellationToken cancellationToken = default);
}