using Leaderboard.Domain.Interfaces.Validation;

namespace Leaderboard.Domain.Dtos.Score;

public record ChangeScoreDto(long StudentId, long CategoryId, int Delta, string? Description) : IValidatableScore;