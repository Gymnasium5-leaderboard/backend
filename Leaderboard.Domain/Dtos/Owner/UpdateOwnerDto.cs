using Leaderboard.Domain.Interfaces.Validation;

namespace Leaderboard.Domain.Dtos.Owner;

public record UpdateOwnerDto(string FirstName, string LastName) : IValidatableOwnerName;
