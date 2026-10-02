namespace Leaderboard.Domain.Dtos.Leaderboard;

public record StudentLeaderboardEntryDto(int Rank, long StudentId, string FirstName, string LastName, int Score);