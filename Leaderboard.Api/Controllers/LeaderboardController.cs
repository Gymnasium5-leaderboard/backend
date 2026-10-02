using Leaderboard.Api.Controllers.Base;
using Leaderboard.Api.Extensions;
using Leaderboard.Domain.Dtos.Leaderboard;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers;

/// <summary>
///     Public leaderboards of the current academic year. Anyone can read them.
/// </summary>
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public class LeaderboardController(ILeaderboardService leaderboardService) : BaseController
{
    /// <summary>
    ///     Gets all classes of the school by average score.
    /// </summary>
    [HttpGet("classes")]
    public async Task<ActionResult<CollectionResult<ClassLeaderboardEntryDto>>> GetSchoolClassesAsync(
        CancellationToken cancellationToken)
    {
        var result = await leaderboardService.GetSchoolClassesAsync(cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Gets classes of one grade by average score.
    /// </summary>
    [HttpGet("grades/{grade:int}/classes")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CollectionResult<ClassLeaderboardEntryDto>>> GetGradeClassesAsync(int grade,
        CancellationToken cancellationToken)
    {
        var result = await leaderboardService.GetGradeClassesAsync(grade, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Gets students of one class by score.
    /// </summary>
    [HttpGet("classes/{classId:long}/students")]
    public async Task<ActionResult<CollectionResult<StudentLeaderboardEntryDto>>> GetClassStudentsAsync(
        long classId, CancellationToken cancellationToken)
    {
        var result = await leaderboardService.GetClassStudentsAsync(classId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Gets rank and score of a student among the classmates.
    /// </summary>
    [HttpGet("students/{studentId:long}")]
    public async Task<ActionResult<BaseResult<StudentRankDto>>> GetStudentRankAsync(long studentId,
        CancellationToken cancellationToken)
    {
        var result = await leaderboardService.GetStudentRankAsync(studentId, cancellationToken);
        return result.ToActionResult();
    }
}