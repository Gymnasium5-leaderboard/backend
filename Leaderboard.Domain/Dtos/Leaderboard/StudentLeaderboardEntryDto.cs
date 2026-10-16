namespace Leaderboard.Domain.Dtos.Leaderboard;

/// <param name="CategoryScores">Scores in every category that is not archived, ordered by id.</param>
public record StudentLeaderboardEntryDto(
    int Rank,
    long StudentId,
    string FirstName,
    string LastName,
    int Score,
    IReadOnlyCollection<StudentCategoryScoreDto> CategoryScores);
