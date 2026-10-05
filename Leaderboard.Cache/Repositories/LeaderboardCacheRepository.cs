using System.Runtime.CompilerServices;
using Leaderboard.Cache.Extensions;
using Leaderboard.Cache.Helpers;
using Leaderboard.Cache.Interfaces;
using Leaderboard.Cache.Settings;
using Leaderboard.Domain.Dtos.Leaderboard;
using Leaderboard.Domain.Interfaces.Repository.Cache;
using Microsoft.Extensions.Options;
using Serilog;

namespace Leaderboard.Cache.Repositories;

/// <summary>
///     Every key contains the version of the leaderboards, and removing all of them only increments it: old keys are
///     no longer read and expire by TTL.
/// </summary>
public class LeaderboardCacheRepository(ICacheProvider cache, IOptions<RedisSettings> redisSettings, ILogger logger)
    : ILeaderboardCacheRepository
{
    private static readonly string VersionKey = CacheKeyHelper.GetLeaderboardVersionKey();

    private readonly int _timeToLiveInSeconds = redisSettings.Value.LeaderboardTimeToLiveInSeconds;

    // Tells "Get failed to read the version" (Set skips) from "no Get was made" (Set reads the version itself)
    private bool _isGetCalled;

    // The version read by the last Get, Set saves under it. Get reads it before the database, so a value loaded before
    // a change goes under the version before it. The repository is scoped, so this belongs to one request.
    private long? _version;

    public Task<IReadOnlyCollection<ClassLeaderboardEntryDto>?> GetSchoolClassesAsync(
        CancellationToken cancellationToken = default)
    {
        return GetAsync<IReadOnlyCollection<ClassLeaderboardEntryDto>>(CacheKeyHelper.GetSchoolClassesLeaderboardKey,
            cancellationToken);
    }

    public Task SetSchoolClassesAsync(IReadOnlyCollection<ClassLeaderboardEntryDto> classes)
    {
        return SetAsync(CacheKeyHelper.GetSchoolClassesLeaderboardKey, classes);
    }

    public Task<IReadOnlyCollection<ClassLeaderboardEntryDto>?> GetGradeClassesAsync(int grade,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<IReadOnlyCollection<ClassLeaderboardEntryDto>>(
            version => CacheKeyHelper.GetGradeClassesLeaderboardKey(version, grade), cancellationToken);
    }

    public Task SetGradeClassesAsync(int grade, IReadOnlyCollection<ClassLeaderboardEntryDto> classes)
    {
        return SetAsync(version => CacheKeyHelper.GetGradeClassesLeaderboardKey(version, grade), classes);
    }

    public Task<IReadOnlyCollection<StudentLeaderboardEntryDto>?> GetClassStudentsAsync(long classId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<IReadOnlyCollection<StudentLeaderboardEntryDto>>(
            version => CacheKeyHelper.GetClassStudentsLeaderboardKey(version, classId), cancellationToken);
    }

    public Task SetClassStudentsAsync(long classId, IReadOnlyCollection<StudentLeaderboardEntryDto> students)
    {
        return SetAsync(version => CacheKeyHelper.GetClassStudentsLeaderboardKey(version, classId), students);
    }

    public Task<StudentRankDto?> GetStudentRankAsync(long studentId, CancellationToken cancellationToken = default)
    {
        return GetAsync<StudentRankDto>(version => CacheKeyHelper.GetStudentRankKey(version, studentId),
            cancellationToken);
    }

    public Task SetStudentRankAsync(StudentRankDto rank)
    {
        return SetAsync(version => CacheKeyHelper.GetStudentRankKey(version, rank.StudentId), rank);
    }

    public async Task RemoveAllAsync()
    {
        try
        {
            await cache.StringIncrementAsync(VersionKey);
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            // The leaderboards expire by TTL, so they are stale at most until then
            logger.LogRedisFailure(e, [VersionKey]);
        }
    }

    private async Task<T?> GetAsync<T>(Func<long, string> getKey, CancellationToken cancellationToken,
        [CallerMemberName] string operation = "") where T : class
    {
        _isGetCalled = true;
        string[] keys = [VersionKey];

        try
        {
            _version = await GetVersionAsync(cancellationToken);

            keys = [getKey(_version.Value)];
            return (await cache.GetJsonParsedAsync<T>(keys, cancellationToken)).SingleOrDefault();
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            logger.LogRedisFailure(e, keys, operation);
            return null;
        }
    }

    private async Task SetAsync<T>(Func<long, string> getKey, T value, [CallerMemberName] string operation = "")
    {
        // Get could not read the version and has already logged it
        if (_isGetCalled && _version == null) return;

        string[] keys = [VersionKey];

        try
        {
            // Without Get, e.g. in a background job, the version is read now. A change made while the value was
            // loaded is then missed until the TTL
            var version = _version ?? await GetVersionAsync();

            keys = [getKey(version)];
            await cache.StringSetAsync([new KeyValuePair<string, T>(keys[0], value)], _timeToLiveInSeconds, true);
        }
        catch (Exception e) when (e.IsRedisFailure())
        {
            logger.LogRedisFailure(e, keys, operation);
        }
    }

    private async Task<long> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        // A missing version is 0: nothing has changed since Redis started
        return (await cache.GetJsonParsedAsync<long>([VersionKey], cancellationToken)).SingleOrDefault();
    }
}