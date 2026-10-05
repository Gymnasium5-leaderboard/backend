using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Interfaces.Repository.Cache;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Results;

namespace Leaderboard.Application.Services.Cache;

public class CacheScoreService(ILeaderboardCacheRepository leaderboardCacheRepository, IScoreService inner)
    : IScoreService
{
    public async Task<BaseResult<ScoreChangedDto>> ChangeScoreAsync(long ownerId, ChangeScoreDto dto,
        Guid? idempotencyKey, CancellationToken cancellationToken = default)
    {
        var result = await inner.ChangeScoreAsync(ownerId, dto, idempotencyKey, cancellationToken);
        if (result.IsSuccess) await leaderboardCacheRepository.RemoveAllAsync();

        return result;
    }

    public async Task<CollectionResult<ScoreChangedDto>> ChangeScoreBatchAsync(long ownerId,
        ChangeScoreBatchDto dto, Guid? idempotencyKey, CancellationToken cancellationToken = default)
    {
        var result = await inner.ChangeScoreBatchAsync(ownerId, dto, idempotencyKey, cancellationToken);
        if (result.IsSuccess) await leaderboardCacheRepository.RemoveAllAsync();

        return result;
    }

    public Task<PagedResult<ScoreTransactionDto>> GetHistoryAsync(ScoreHistoryQueryDto query,
        CancellationToken cancellationToken = default)
    {
        return inner.GetHistoryAsync(query, cancellationToken);
    }
}