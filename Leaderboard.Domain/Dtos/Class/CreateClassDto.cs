using Leaderboard.Domain.Interfaces.Validation;

namespace Leaderboard.Domain.Dtos.Class;

public record CreateClassDto(int Grade, char Letter) : IValidatableClass;
