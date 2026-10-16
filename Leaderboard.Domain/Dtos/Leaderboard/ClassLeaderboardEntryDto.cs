namespace Leaderboard.Domain.Dtos.Leaderboard;

/// <param name="Score">Average score of the active students of the class.</param>
/// <param name="CategoryScores">Scores in every category that is not archived, ordered by id.</param>
public record ClassLeaderboardEntryDto(
    int Rank,
    long ClassId,
    string ClassName,
    double Score,
    IReadOnlyCollection<ClassCategoryScoreDto> CategoryScores);
