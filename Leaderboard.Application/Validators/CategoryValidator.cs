using FluentValidation;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Domain.Settings;

namespace Leaderboard.Application.Validators;

public class CategoryValidator : AbstractValidator<IValidatableCategory>
{
    public CategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_ => InvalidNameMessage())
            .MaximumLength(EntityConstraints.CategoryNameMaxLength).WithMessage(_ => InvalidNameMessage());
    }

    // Formatted on each validation, so the message follows the request culture
    private static string InvalidNameMessage() =>
        string.Format(ErrorMessage.InvalidCategoryName, EntityConstraints.CategoryNameMaxLength);
}