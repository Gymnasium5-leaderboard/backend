namespace Leaderboard.Domain.Interfaces.Validation;

public interface IValidatableClass
{
    public int Grade { get; }
    public char Letter { get; }
    public string ClassTeacherName { get; }
}
