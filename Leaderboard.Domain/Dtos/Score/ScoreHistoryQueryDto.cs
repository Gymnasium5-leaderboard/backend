namespace Leaderboard.Domain.Dtos.Score;

/// <summary>
///     Filter and page of the score history. Without filters, the whole school history is returned.
/// </summary>
public record ScoreHistoryQueryDto(long? StudentId, long? ClassId, int Page = 1, int PageSize = 20);