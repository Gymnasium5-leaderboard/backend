namespace Leaderboard.Domain.Dtos.Score;

/// <param name="Score">Current score of the student in this academic year</param>
public record ScoreChangedDto(long TransactionId, long StudentId, int Delta, int Score, DateTime CreatedAt);