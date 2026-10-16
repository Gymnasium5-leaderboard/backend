namespace Leaderboard.Domain.Dtos.Score;

/// <param name="Score">Current score of the student in this academic year, over the categories that are not archived</param>
/// <param name="CategoryScore">Current score of the student in the category of the transaction</param>
public record ScoreChangedDto(
    long TransactionId,
    long StudentId,
    long CategoryId,
    int Delta,
    int Score,
    int CategoryScore,
    DateTime CreatedAt);
