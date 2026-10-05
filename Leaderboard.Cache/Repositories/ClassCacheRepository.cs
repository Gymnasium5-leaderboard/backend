using Leaderboard.Cache.Extensions;
using Leaderboard.Cache.Helpers;
using Leaderboard.Cache.Interfaces;
using Leaderboard.Cache.Settings;
using Leaderboard.Domain.Dtos.Class;
using Leaderboard.Domain.Interfaces.Repository.Cache;
using Leaderboard.Domain.Settings;
using Microsoft.Extensions.Options;
using Serilog;

namespace Leaderboard.Cache.Repositories;

public class ClassCacheRepository(ICacheProvider cache, IOptions<RedisSettings> redisSettings, ILogger logger)
    : IClassCacheRepository
{
    private static readonly int[] AllGrades = Enumerable
        .Range(EntityConstraints.MinGrade, EntityConstraints.MaxGrade - EntityConstraints.MinGrade + 1)
        .ToArray();

    private readonly int _timeToLiveInSeconds = redisSettings.Value.TimeToLiveInSeconds;

    public async Task<ClassDto?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var key = CacheKeyHelper.GetClassKey(id);

        try
        {
            return (await cache.GetJsonParsedAsync<ClassDto>([key], cancellationToken)).SingleOrDefault();
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            logger.LogRedisFailure(e, [key]);
            return null;
        }
    }

    public async Task SetAsync(ClassDto schoolClass)
    {
        try
        {
            await SetClassesAsync([schoolClass]);
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            logger.LogRedisFailure(e, [CacheKeyHelper.GetClassKey(schoolClass.Id)]);
        }
    }

    public async Task<IReadOnlyCollection<ClassDto>?> GetByGradeAsync(int? grade,
        CancellationToken cancellationToken = default)
    {
        var listKey = CacheKeyHelper.GetGradeClassesKey(grade);

        try
        {
            var ids = (await cache.GetJsonParsedAsync<long[]>([listKey], cancellationToken)).SingleOrDefault();
            if (ids == null || ids.Length == 0) return null;

            var classes = (await cache.GetJsonParsedAsync<ClassDto>(ids.Select(CacheKeyHelper.GetClassKey),
                cancellationToken)).ToDictionary(x => x.Id);

            // A class of the list has expired, so the whole list is fetched again
            if (classes.Count != ids.Length) return null;

            return ids.Select(x => classes[x]).ToArray();
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            logger.LogRedisFailure(e, [listKey]);
            return null;
        }
    }

    public async Task SetByGradeAsync(int? grade, IReadOnlyCollection<ClassDto> classes)
    {
        var listKey = CacheKeyHelper.GetGradeClassesKey(grade);

        try
        {
            // Classes go first, so a list never points to a class that is not cached yet
            await SetClassesAsync(classes);
            await cache.StringSetAsync([new KeyValuePair<string, long[]>(listKey, [.. classes.Select(x => x.Id)])],
                _timeToLiveInSeconds, true);
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            logger.LogRedisFailure(e, [listKey]);
        }
    }

    public Task RemoveAsync(long id)
    {
        // The class may have moved to another grade, and its old grade is unknown here
        return RemoveKeysAsync([CacheKeyHelper.GetClassKey(id), .. GetGradeListKeys(AllGrades)]);
    }

    public Task RemoveGradesAsync(IReadOnlyCollection<int> grades)
    {
        return RemoveKeysAsync(GetGradeListKeys(grades));
    }

    public async Task RemoveAllAsync()
    {
        var patterns = CacheKeyHelper.GetAllClassesKeyPatterns();

        try
        {
            await cache.KeysDeleteByPatternsAsync(patterns);
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            // The keys expire by TTL, so the cache is stale at most until then
            logger.LogRedisFailure(e, patterns);
        }
    }

    // The list of all classes contains every grade, so it is removed together with any of them
    private static string[] GetGradeListKeys(IEnumerable<int> grades)
    {
        return
        [
            CacheKeyHelper.GetGradeClassesKey(null),
            .. grades.Distinct().Select(x => CacheKeyHelper.GetGradeClassesKey(x))
        ];
    }

    private Task SetClassesAsync(IEnumerable<ClassDto> classes)
    {
        return cache.StringSetAsync(
            classes.Select(x => new KeyValuePair<string, ClassDto>(CacheKeyHelper.GetClassKey(x.Id), x)),
            _timeToLiveInSeconds, true);
    }

    private async Task RemoveKeysAsync(string[] keys)
    {
        try
        {
            await cache.KeysDeleteAsync(keys);
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            // The keys expire by TTL, so the cache is stale at most until then
            logger.LogRedisFailure(e, keys);
        }
    }
}