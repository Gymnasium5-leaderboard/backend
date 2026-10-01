using FluentValidation;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Interfaces.Validation;
using Leaderboard.Domain.Settings;
using Microsoft.Extensions.Options;

namespace Leaderboard.Application.Validators;

public class ScoreValidator : AbstractValidator<IValidatableScore>
{
    public ScoreValidator(IOptions<BusinessRules> businessRules)
    {
        var maxDelta = businessRules.Value.MaxScoreDelta;

        RuleFor(x => x.Delta).NotEqual(0).WithMessage(_ => InvalidDeltaMessage(maxDelta))
            .InclusiveBetween(-maxDelta, maxDelta).WithMessage(_ => InvalidDeltaMessage(maxDelta));
        RuleFor(x => x.Description).MaximumLength(EntityConstraints.ScoreDescriptionMaxLength)
            .WithMessage(_ => string.Format(ErrorMessage.InvalidScoreDescription,
                EntityConstraints.ScoreDescriptionMaxLength));
    }

    // Formatted on each validation, so the message follows the request culture
    private static string InvalidDeltaMessage(int maxDelta)
    {
        return string.Format(ErrorMessage.InvalidScoreDelta, maxDelta);
    }
}