using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Results;

namespace Leaderboard.Domain.Interfaces.Service;

public interface IScoreService
{
    /// <summary>
    ///     Changes the score of one active student. A repeated request with the same idempotency key returns the
    ///     transaction created by the first one.
    /// </summary>
    Task<BaseResult<ScoreChangedDto>> ChangeScoreAsync(long ownerId, ChangeScoreDto dto, Guid? idempotencyKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Changes the score of several students or a whole class at once. If any student fails the checks, nothing
    ///     is changed.
    /// </summary>
    Task<CollectionResult<ScoreChangedDto>> ChangeScoreBatchAsync(long ownerId, ChangeScoreBatchDto dto,
        Guid? idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Score transactions of the current academic year, newest first.
    /// </summary>
    Task<PagedResult<ScoreTransactionDto>> GetHistoryAsync(ScoreHistoryQueryDto query,
        CancellationToken cancellationToken = default);
}