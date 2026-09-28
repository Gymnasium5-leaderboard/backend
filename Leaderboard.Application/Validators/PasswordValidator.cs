using FluentValidation;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Domain.Settings;

namespace Leaderboard.Application.Validators;

public class PasswordValidator : AbstractValidator<IValidatableOwnerPassword>
{
    public PasswordValidator()
    {
        RuleFor(x => x.Password)
            .NotNull().WithMessage(_ => InvalidPasswordMessage())
            .Length(EntityConstraints.PasswordMinLength, EntityConstraints.PasswordMaxLength)
            .WithMessage(_ => InvalidPasswordMessage());
    }

    // Formatted on each validation, so the message follows the request culture
    private static string InvalidPasswordMessage() => string.Format(ErrorMessage.InvalidPassword,
        EntityConstraints.PasswordMinLength, EntityConstraints.PasswordMaxLength);
}