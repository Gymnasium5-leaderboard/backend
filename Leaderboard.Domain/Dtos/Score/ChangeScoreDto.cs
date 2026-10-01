using Leaderboard.Domain.Interfaces.Validation;

namespace Leaderboard.Domain.Dtos.Score;

public record ChangeScoreDto(long StudentId, int Delta, string? Description) : IValidatableScore;