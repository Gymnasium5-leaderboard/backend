using FluentValidation;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Owner;
using Leaderboard.Domain.Settings;

namespace Leaderboard.Application.Validators;

public class CreateOwnerValidator : AbstractValidator<CreateOwnerDto>
{
    public CreateOwnerValidator()
    {
        RuleFor(x => x.Login)
            .NotNull().WithMessage(_ => InvalidLoginMessage())
            .Length(EntityConstraints.LoginMinLength, EntityConstraints.LoginMaxLength)
            .WithMessage(_ => InvalidLoginMessage())
            .Matches("^[A-Za-z0-9._-]+$").WithMessage(_ => InvalidLoginMessage());
        Include(new PasswordValidator());
        Include(new NameValidator());
    }

    // Formatted on each validation, so the message follows the request culture
    private static string InvalidLoginMessage() => string.Format(ErrorMessage.InvalidLogin,
        EntityConstraints.LoginMinLength, EntityConstraints.LoginMaxLength);
}