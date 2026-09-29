using FluentValidation;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Domain.Settings;

namespace Leaderboard.Application.Validators;

public class ClassValidator : AbstractValidator<IValidatableClass>
{
    public ClassValidator()
    {
        RuleFor(x => x.Grade).InclusiveBetween(EntityConstraints.MinGrade, EntityConstraints.MaxGrade)
            .WithMessage(_ => InvalidGradeMessage());
        RuleFor(x => x.Letter).Must(char.IsLetter).WithMessage(_ => ErrorMessage.InvalidClassLetter);
    }

    // Formatted on each validation, so the message follows the request culture
    private static string InvalidGradeMessage() => string.Format(ErrorMessage.InvalidGrade,
        EntityConstraints.MinGrade, EntityConstraints.MaxGrade);
}
