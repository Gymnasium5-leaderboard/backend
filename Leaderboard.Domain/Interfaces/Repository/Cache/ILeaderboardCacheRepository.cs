using Leaderboard.Domain.Dtos.Leaderboard;

namespace Leaderboard.Domain.Interfaces.Repository.Cache;

/// <summary>
///     Leaderboards are cached as they are returned. Any change of scores, students or classes removes all of them.
///     Cache errors are logged, not thrown: a failed read is a miss.
/// </summary>
public interface ILeaderboardCacheRepository
{
    /// <summary>
    ///     All classes of the school, or null on a cache miss.
    /// </summary>
    Task<IReadOnlyCollection<ClassLeaderboardEntryDto>?> GetSchoolClassesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Caches a leaderboard under the version read by the last Get of this repository in the same scope. If the
    ///     data changed after that Get, the leaderboard goes under the old version, which is no longer read, so a
    ///     leaderboard loaded before a change is never returned after it. Without a Get the current version is used.
    /// </summary>
    Task SetSchoolClassesAsync(IReadOnlyCollection<ClassLeaderboardEntryDto> classes);

    /// <summary>
    ///     Classes of one grade, or null on a cache miss.
    /// </summary>
    Task<IReadOnlyCollection<ClassLeaderboardEntryDto>?> GetGradeClassesAsync(int grade,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Caches a leaderboard under the version read by the last Get of this repository in the same scope. If the
    ///     data changed after that Get, the leaderboard goes under the old version, which is no longer read, so a
    ///     leaderboard loaded before a change is never returned after it. Without a Get the current version is used.
    /// </summary>
    Task SetGradeClassesAsync(int grade, IReadOnlyCollection<ClassLeaderboardEntryDto> classes);

    /// <summary>
    ///     Students of one class, or null on a cache miss.
    /// </summary>
    Task<IReadOnlyCollection<StudentLeaderboardEntryDto>?> GetClassStudentsAsync(long classId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Caches a leaderboard under the version read by the last Get of this repository in the same scope. If the
    ///     data changed after that Get, the leaderboard goes under the old version, which is no longer read, so a
    ///     leaderboard loaded before a change is never returned after it. Without a Get the current version is used.
    /// </summary>
    Task SetClassStudentsAsync(long classId, IReadOnlyCollection<StudentLeaderboardEntryDto> students);

    /// <summary>
    ///     Rank of a student, or null on a cache miss.
    /// </summary>
    Task<StudentRankDto?> GetStudentRankAsync(long studentId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Caches a leaderboard under the version read by the last Get of this repository in the same scope. If the
    ///     data changed after that Get, the leaderboard goes under the old version, which is no longer read, so a
    ///     leaderboard loaded before a change is never returned after it. Without a Get the current version is used.
    /// </summary>
    Task SetStudentRankAsync(StudentRankDto rank);

    /// <summary>
    ///     Removes all cached leaderboards.
    /// </summary>
    Task RemoveAllAsync();
}