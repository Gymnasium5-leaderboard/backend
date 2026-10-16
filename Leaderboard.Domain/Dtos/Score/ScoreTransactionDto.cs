namespace Leaderboard.Domain.Dtos.Score;

public record ScoreTransactionDto(
    long Id,
    long StudentId,
    string StudentFirstName,
    string StudentLastName,
    long OwnerId,
    string OwnerFirstName,
    string OwnerLastName,
    long CategoryId,
    string CategoryName,
    bool CategoryIsArchived,
    int Delta,
    string? Description,
    DateTime CreatedAt);