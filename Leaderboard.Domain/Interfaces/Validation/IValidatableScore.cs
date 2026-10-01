namespace Leaderboard.Domain.Interfaces.Validation;

public interface IValidatableScore
{
    public int Delta { get; }
    public string? Description { get; }
}