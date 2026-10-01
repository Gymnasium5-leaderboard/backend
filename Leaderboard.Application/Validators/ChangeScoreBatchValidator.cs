using FluentValidation;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Settings;
using Microsoft.Extensions.Options;

namespace Leaderboard.Application.Validators;

public class ChangeScoreBatchValidator : AbstractValidator<ChangeScoreBatchDto>
{
    public ChangeScoreBatchValidator(IOptions<BusinessRules> businessRules)
    {
        RuleFor(x => x).Must(x => x.StudentIds is { Count: > 0 } ^ (x.ClassId != null))
            .WithMessage(_ => ErrorMessage.InvalidScoreTarget);
        Include(new ScoreValidator(businessRules));
    }
}