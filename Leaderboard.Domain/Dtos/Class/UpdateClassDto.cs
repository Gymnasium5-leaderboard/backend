using Leaderboard.Domain.Interfaces.Validation;

namespace Leaderboard.Domain.Dtos.Class;

public record UpdateClassDto(int Grade, char Letter) : IValidatableClass;
