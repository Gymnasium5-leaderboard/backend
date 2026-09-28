using Leaderboard.Domain.Interfaces.Validation;

namespace Leaderboard.Domain.Dtos.Auth;

public record ChangePasswordDto(string CurrentPassword, string Password) : IValidatableOwnerPassword;