using Leaderboard.Api.Controllers.Base;
using Leaderboard.Api.Extensions;
using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers;

/// <summary>
///     Score changes and their history. Only owners change and see them.
/// </summary>
[Authorize]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ScoreController(IScoreService scoreService) : BaseController
{
    private const string IdempotencyKeyHeader = "Idempotency-Key";

    /// <summary>
    ///     Changes the score of a student.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BaseResult<ScoreChangedDto>>> ChangeScoreAsync(ChangeScoreDto dto,
        [FromHeader(Name = IdempotencyKeyHeader)]
        Guid? idempotencyKey, CancellationToken cancellationToken)
    {
        var result = await scoreService.ChangeScoreAsync(User.GetOwnerId(), dto, idempotencyKey, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Changes the score of a list of students or of all students of a class. If any student fails the checks,
    ///     nothing is changed.
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CollectionResult<ScoreChangedDto>>> ChangeScoreBatchAsync(
        ChangeScoreBatchDto dto, [FromHeader(Name = IdempotencyKeyHeader)] Guid? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var result =
            await scoreService.ChangeScoreBatchAsync(User.GetOwnerId(), dto, idempotencyKey, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    ///     Gets score changes of the current academic year, newest first, by student, by class or for the whole school.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CollectionResult<ScoreTransactionDto>>> GetHistoryAsync(
        [FromQuery] ScoreHistoryQueryDto query, CancellationToken cancellationToken)
    {
        var result = await scoreService.GetHistoryAsync(query, cancellationToken);
        return result.ToActionResult();
    }
}