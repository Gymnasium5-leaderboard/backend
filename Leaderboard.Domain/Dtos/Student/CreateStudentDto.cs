using Leaderboard.Domain.Interfaces.Validation;

namespace Leaderboard.Domain.Dtos.Student;

public record CreateStudentDto(string FirstName, string LastName, long ClassId) : IValidatableName;
