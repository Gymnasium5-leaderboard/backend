namespace Leaderboard.Domain.Dtos.Leaderboard;

/// <param name="Rank">Rank among the classmates.</param>
/// <param name="CategoryScores">Scores in every category that is not archived, ordered by id.</param>
public record StudentRankDto(
    int Rank,
    long StudentId,
    string FirstName,
    string LastName,
    int Score,
    long ClassId,
    string ClassName,
    IReadOnlyCollection<StudentCategoryScoreDto> CategoryScores);
