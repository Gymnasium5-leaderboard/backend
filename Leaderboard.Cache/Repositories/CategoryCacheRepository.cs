using Leaderboard.Cache.Extensions;
using Leaderboard.Cache.Helpers;
using Leaderboard.Cache.Interfaces;
using Leaderboard.Cache.Settings;
using Leaderboard.Domain.Dtos.Category;
using Leaderboard.Domain.Interfaces.Repository.Cache;
using Microsoft.Extensions.Options;
using Serilog;

namespace Leaderboard.Cache.Repositories;

public class CategoryCacheRepository(
    ICacheProvider cache,
    IOptions<RedisSettings> redisSettings,
    ILogger logger) : ICategoryCacheRepository
{
    private static readonly string CategoriesKey = CacheKeyHelper.GetCategoriesKey();

    public async Task<IReadOnlyCollection<CategoryDto>?> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return (await cache.GetJsonParsedAsync<CategoryDto[]>([CategoriesKey], cancellationToken))
                .SingleOrDefault();
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            logger.LogRedisFailure(e, [CategoriesKey]);
            return null;
        }
    }

    public async Task SetAllAsync(IReadOnlyCollection<CategoryDto> categories)
    {
        try
        {
            await cache.StringSetAsync([new KeyValuePair<string, IReadOnlyCollection<CategoryDto>>(CategoriesKey,
                categories)], redisSettings.Value.TimeToLiveInSeconds, true);
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            logger.LogRedisFailure(e, [CategoriesKey]);
        }
    }

    public async Task RemoveAllAsync()
    {
        try
        {
            await cache.KeysDeleteAsync([CategoriesKey]);
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            // The key expires by TTL, so the cache is stale at most until then
            logger.LogRedisFailure(e, [CategoriesKey]);
        }
    }
}
