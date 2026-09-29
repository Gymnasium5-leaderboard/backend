namespace Leaderboard.Domain.Interfaces.Validation;

/// <summary>
///     First and last name of an owner or a student.
/// </summary>
public interface IValidatableName
{
    public string FirstName { get; }
    public string LastName { get; }
}
