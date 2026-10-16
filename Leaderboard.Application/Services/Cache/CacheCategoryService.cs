using Leaderboard.Domain.Dtos.Category;
using Leaderboard.Domain.Interfaces.Repository.Cache;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;

namespace Leaderboard.Application.Services.Cache;

public class CacheCategoryService(
    ICategoryCacheRepository cacheRepository,
    ILeaderboardCacheRepository leaderboardCacheRepository,
    ICategoryService inner) : ICategoryService
{
    public async Task<CollectionResult<CategoryDto>> GetAllAsync(bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAllCachedAsync(cancellationToken);
        if (!result.IsSuccess || includeArchived) return result;

        var filtered = result.Data.Where(x => !x.IsArchived).ToArray();

        return CollectionResult<CategoryDto>.Success(filtered);
    }

    public async Task<BaseResult<CategoryDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var result = await GetAllCachedAsync(cancellationToken);
        var category = result.Data?.FirstOrDefault(x => x.Id == id);
       
        return category != null
            ? BaseResult<CategoryDto>.Success(category)
            : await inner.GetByIdAsync(id, cancellationToken);
    }

    public async Task<BaseResult<CategoryDto>> CreateAsync(CreateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await inner.CreateAsync(dto, cancellationToken);
        if (result.IsSuccess) await RemoveAllAsync();

        return result;
    }

    public async Task<BaseResult<CategoryDto>> UpdateAsync(long id, UpdateCategoryDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await inner.UpdateAsync(id, dto, cancellationToken);
        if (result.IsSuccess) await RemoveAllAsync();

        return result;
    }

    public async Task<BaseResult<CategoryDto>> ArchiveAsync(long id, CancellationToken cancellationToken = default)
    {
        var result = await inner.ArchiveAsync(id, cancellationToken);
        if (result.IsSuccess) await RemoveAllAsync();

        return result;
    }

    public async Task<BaseResult<CategoryDto>> RestoreAsync(long id, CancellationToken cancellationToken = default)
    {
        var result = await inner.RestoreAsync(id, cancellationToken);
        if (result.IsSuccess) await RemoveAllAsync();

        return result;
    }

    public async Task<BaseResult> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var result = await inner.DeleteAsync(id, cancellationToken);
        if (result.IsSuccess) await RemoveAllAsync();

        return result;
    }

    // All categories, archived ones too: from the cache, or loaded and cached on a miss
    private async Task<CollectionResult<CategoryDto>> GetAllCachedAsync(CancellationToken cancellationToken)
    {
        var cached = await cacheRepository.GetAllAsync(cancellationToken);
        if (cached != null) return CollectionResult<CategoryDto>.Success(cached);

        var result = await inner.GetAllAsync(true, cancellationToken);
        if (result.IsSuccess) await cacheRepository.SetAllAsync(result.Data);

        return result;
    }

    private async Task RemoveAllAsync()
    {
        await cacheRepository.RemoveAllAsync();
        await leaderboardCacheRepository.RemoveAllAsync();
    }
}
