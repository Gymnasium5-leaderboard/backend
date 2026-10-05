using Leaderboard.Cache.Extensions;
using Leaderboard.Cache.Helpers;
using Leaderboard.Cache.Interfaces;
using Leaderboard.Cache.Settings;
using Leaderboard.Domain.Dtos.AcademicYear;
using Leaderboard.Domain.Interfaces.Repository.Cache;
using Microsoft.Extensions.Options;
using Serilog;

namespace Leaderboard.Cache.Repositories;

public class AcademicYearCacheRepository(
    ICacheProvider cache,
    IOptions<RedisSettings> redisSettings,
    ILogger logger) : IAcademicYearCacheRepository
{
    private static readonly string CurrentKey = CacheKeyHelper.GetCurrentAcademicYearKey();

    public async Task<AcademicYearDto?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return (await cache.GetJsonParsedAsync<AcademicYearDto>([CurrentKey], cancellationToken)).SingleOrDefault();
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            logger.LogRedisFailure(e, [CurrentKey]);
            return null;
        }
    }

    public async Task SetCurrentAsync(AcademicYearDto year)
    {
        try
        {
            await cache.StringSetAsync([new KeyValuePair<string, AcademicYearDto>(CurrentKey, year)],
                redisSettings.Value.TimeToLiveInSeconds, true);
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            logger.LogRedisFailure(e, [CurrentKey]);
        }
    }

    public async Task RemoveCurrentAsync()
    {
        try
        {
            await cache.KeysDeleteAsync([CurrentKey]);
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            // The key expires by TTL, so the cache is stale at most until then
            logger.LogRedisFailure(e, [CurrentKey]);
        }
    }
}