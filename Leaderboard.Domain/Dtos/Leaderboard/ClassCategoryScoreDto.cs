namespace Leaderboard.Domain.Dtos.Leaderboard;

/// <param name="Score">Average score of the active students of the class in the category, rounded to 2 digits.</param>
public record ClassCategoryScoreDto(long CategoryId, string CategoryName, double Score);
