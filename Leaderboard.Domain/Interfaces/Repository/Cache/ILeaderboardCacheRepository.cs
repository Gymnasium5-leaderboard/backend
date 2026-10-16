using Leaderboard.Domain.Dtos.Leaderboard;

namespace Leaderboard.Domain.Interfaces.Repository.Cache;

/// <summary>
///     Leaderboards are cached as they are returned, for all categories (<c>categoryId</c> is null) or for one. Any
///     change of scores, students, classes or categories removes all of them.
///     Cache errors are logged, not thrown: a failed read is a miss.
/// </summary>
public interface ILeaderboardCacheRepository
{
    /// <summary>
    ///     All classes of the school, or null on a cache miss.
    /// </summary>
    Task<IReadOnlyCollection<ClassLeaderboardEntryDto>?> GetSchoolClassesAsync(long? categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Caches a leaderboard under the version read by the last Get of this repository in the same scope. If the
    ///     data changed after that Get, the leaderboard goes under the old version, which is no longer read, so a
    ///     leaderboard loaded before a change is never returned after it. Without a Get the current version is used.
    /// </summary>
    Task SetSchoolClassesAsync(long? categoryId, IReadOnlyCollection<ClassLeaderboardEntryDto> classes);

    /// <summary>
    ///     Classes of one grade, or null on a cache miss.
    /// </summary>
    Task<IReadOnlyCollection<ClassLeaderboardEntryDto>?> GetGradeClassesAsync(int grade, long? categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Caches a leaderboard under the version read by the last Get of this repository in the same scope. If the
    ///     data changed after that Get, the leaderboard goes under the old version, which is no longer read, so a
    ///     leaderboard loaded before a change is never returned after it. Without a Get the current version is used.
    /// </summary>
    Task SetGradeClassesAsync(int grade, long? categoryId, IReadOnlyCollection<ClassLeaderboardEntryDto> classes);

    /// <summary>
    ///     Students of one class, or null on a cache miss.
    /// </summary>
    Task<IReadOnlyCollection<StudentLeaderboardEntryDto>?> GetClassStudentsAsync(long classId, long? categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Caches a leaderboard under the version read by the last Get of this repository in the same scope. If the
    ///     data changed after that Get, the leaderboard goes under the old version, which is no longer read, so a
    ///     leaderboard loaded before a change is never returned after it. Without a Get the current version is used.
    /// </summary>
    Task SetClassStudentsAsync(long classId, long? categoryId,
        IReadOnlyCollection<StudentLeaderboardEntryDto> students);

    /// <summary>
    ///     Rank of a student, or null on a cache miss.
    /// </summary>
    Task<StudentRankDto?> GetStudentRankAsync(long studentId, long? categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Caches a leaderboard under the version read by the last Get of this repository in the same scope. If the
    ///     data changed after that Get, the leaderboard goes under the old version, which is no longer read, so a
    ///     leaderboard loaded before a change is never returned after it. Without a Get the current version is used.
    /// </summary>
    Task SetStudentRankAsync(long? categoryId, StudentRankDto rank);

    /// <summary>
    ///     Removes all cached leaderboards.
    /// </summary>
    Task RemoveAllAsync();
}