namespace Leaderboard.Domain.Dtos.Leaderboard;

public record StudentLeaderboardEntryDto(int Place, long StudentId, string FirstName, string LastName, int Score);