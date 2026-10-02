using Leaderboard.Domain.Dtos.Leaderboard;
using Leaderboard.Domain.Results;

namespace Leaderboard.Domain.Interfaces.Service;

/// <summary>
///     Leaderboards of the current academic year. Only active students and classes take part, classes without students
///     are not shown. With equal score, the one who reached it earlier is higher.
/// </summary>
public interface ILeaderboardService
{
    /// <summary>
    ///     All classes of the school by average score.
    /// </summary>
    Task<CollectionResult<ClassLeaderboardEntryDto>> GetSchoolClassesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Classes of one grade by average score.
    /// </summary>
    Task<CollectionResult<ClassLeaderboardEntryDto>> GetGradeClassesAsync(int grade,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Students of one class by score.
    /// </summary>
    Task<CollectionResult<StudentLeaderboardEntryDto>> GetClassStudentsAsync(long classId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Place and score of a student among the classmates.
    /// </summary>
    Task<BaseResult<StudentPlaceDto>> GetStudentPlaceAsync(long studentId,
        CancellationToken cancellationToken = default);
}