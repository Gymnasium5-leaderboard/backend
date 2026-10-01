using Leaderboard.Domain.Interfaces.Validation;

namespace Leaderboard.Domain.Dtos.Score;

/// <summary>
///     The same change for a list of students or for all active students of a class. Exactly one of them is set.
/// </summary>
public record ChangeScoreBatchDto(IReadOnlyCollection<long>? StudentIds, long? ClassId, int Delta, string? Description)
    : IValidatableScore;