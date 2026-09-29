using Leaderboard.Domain.Interfaces.Validation;

namespace Leaderboard.Domain.Dtos.Student;

public record UpdateStudentDto(string FirstName, string LastName) : IValidatableName;
