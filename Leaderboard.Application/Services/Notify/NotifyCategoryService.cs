using Leaderboard.Domain.Dtos.Category;
using Leaderboard.Domain.Interfaces.Notifier;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;

namespace Leaderboard.Application.Services.Notify;

/// <summary>
///     Any change of a category changes the category scores of the leaderboards, archive and restore change the
///     scores too.
/// </summary>
public class NotifyCategoryService(ILeaderboardNotifier leaderboardNotifier, ICategoryService inner)
    : ICategoryService
{
    public Task<CollectionResult<CategoryDto>> GetAllAsync(bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        return inner.GetAllAsync(includeArchived, cancellationToken);
    }

    public Task<BaseResult<CategoryDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return inner.GetByIdAsync(id, cancellationToken);
    }

    public async Task<BaseResult<CategoryDto>> CreateAsync(CreateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await inner.CreateAsync(dto, cancellationToken);
        if (result.IsSuccess) await leaderboardNotifier.NotifyChangedAsync();

        return result;
    }

    public async Task<BaseResult<CategoryDto>> UpdateAsync(long id, UpdateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await inner.UpdateAsync(id, dto, cancellationToken);
        if (result.IsSuccess) await leaderboardNotifier.NotifyChangedAsync();

        return result;
    }

    public async Task<BaseResult<CategoryDto>> ArchiveAsync(long id, CancellationToken cancellationToken = default)
    {
        var result = await inner.ArchiveAsync(id, cancellationToken);
        if (result.IsSuccess) await leaderboardNotifier.NotifyChangedAsync();

        return result;
    }

    public async Task<BaseResult<CategoryDto>> RestoreAsync(long id, CancellationToken cancellationToken = default)
    {
        var result = await inner.RestoreAsync(id, cancellationToken);
        if (result.IsSuccess) await leaderboardNotifier.NotifyChangedAsync();

        return result;
    }

    public async Task<BaseResult> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var result = await inner.DeleteAsync(id, cancellationToken);
        if (result.IsSuccess) await leaderboardNotifier.NotifyChangedAsync();

        return result;
    }
}
