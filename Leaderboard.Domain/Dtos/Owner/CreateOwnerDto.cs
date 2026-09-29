using Leaderboard.Domain.Interfaces.Validation;

namespace Leaderboard.Domain.Dtos.Owner;

/// <param name="CreatorPassword">Password of the owner who creates, so a stolen access token is not enough</param>
public record CreateOwnerDto(string Login, string Password, string FirstName, string LastName, string CreatorPassword)
    : IValidatableName, IValidatableOwnerPassword;