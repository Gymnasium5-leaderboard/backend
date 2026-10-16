using Leaderboard.Domain.Dtos.Leaderboard;
using Leaderboard.Domain.Results;

namespace Leaderboard.Domain.Interfaces.Service;

/// <summary>
///     Leaderboards of the current academic year. Only active students and classes take part, classes without students
///     are not shown. With equal score, the one who reached it earlier is higher. The score is over the categories
///     that are not archived, or over one of them if <c>categoryId</c> is set. Every entry has the scores in each
///     category that is not archived.
/// </summary>
public interface ILeaderboardService
{
    /// <summary>
    ///     All classes of the school by average score.
    /// </summary>
    Task<CollectionResult<ClassLeaderboardEntryDto>> GetSchoolClassesAsync(long? categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Classes of one grade by average score.
    /// </summary>
    Task<CollectionResult<ClassLeaderboardEntryDto>> GetGradeClassesAsync(int grade, long? categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Students of one class by score.
    /// </summary>
    Task<CollectionResult<StudentLeaderboardEntryDto>> GetClassStudentsAsync(long classId, long? categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rank and score of a student among the classmates.
    /// </summary>
    Task<BaseResult<StudentRankDto>> GetStudentRankAsync(long studentId, long? categoryId,
        CancellationToken cancellationToken = default);
}
