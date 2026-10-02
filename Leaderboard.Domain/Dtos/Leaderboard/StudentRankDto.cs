namespace Leaderboard.Domain.Dtos.Leaderboard;

/// <param name="Rank">Rank among the classmates.</param>
public record StudentRankDto(
    int Rank,
    long StudentId,
    string FirstName,
    string LastName,
    int Score,
    long ClassId,
    string ClassName);