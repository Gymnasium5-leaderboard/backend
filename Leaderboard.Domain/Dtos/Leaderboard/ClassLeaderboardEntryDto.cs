namespace Leaderboard.Domain.Dtos.Leaderboard;

/// <param name="Score">Average score of the active students of the class.</param>
public record ClassLeaderboardEntryDto(int Place, long ClassId, string ClassName, double Score);