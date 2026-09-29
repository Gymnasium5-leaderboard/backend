using Leaderboard.Domain.Interfaces.Validation;

namespace Leaderboard.Domain.Dtos.Owner;

public record CreateOwnerDto(string Login, string Password, string FirstName, string LastName)
    : IValidatableName, IValidatableOwnerPassword;