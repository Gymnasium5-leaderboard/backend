using FluentValidation;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Domain.Settings;

namespace Leaderboard.Application.Validators;

public class NameValidator : AbstractValidator<IValidatableName>
{
    public NameValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage(_ => InvalidFirstNameMessage())
            .MaximumLength(EntityConstraints.NameMaxLength)
            .WithMessage(_ => InvalidFirstNameMessage());
        RuleFor(x => x.LastName).NotEmpty().WithMessage(_ => InvalidLastNameMessage())
            .MaximumLength(EntityConstraints.NameMaxLength)
            .WithMessage(_ => InvalidLastNameMessage());
    }

    // Formatted on each validation, so the message follows the request culture
    private static string InvalidFirstNameMessage() =>
        string.Format(ErrorMessage.InvalidFirstName, EntityConstraints.NameMaxLength);

    private static string InvalidLastNameMessage() =>
        string.Format(ErrorMessage.InvalidLastName, EntityConstraints.NameMaxLength);
}