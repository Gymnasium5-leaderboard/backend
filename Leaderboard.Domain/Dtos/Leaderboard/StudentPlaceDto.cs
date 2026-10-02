namespace Leaderboard.Domain.Dtos.Leaderboard;

/// <param name="Place">Place among the classmates.</param>
public record StudentPlaceDto(
    int Place,
    long StudentId,
    string FirstName,
    string LastName,
    int Score,
    long ClassId,
    string ClassName);