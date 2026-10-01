using FluentValidation;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Dtos.Score;
using Leaderboard.Domain.Settings;

namespace Leaderboard.Application.Validators;

public class ScoreHistoryQueryValidator : AbstractValidator<ScoreHistoryQueryDto>
{
    public ScoreHistoryQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage(_ => ErrorMessage.InvalidPage);
        RuleFor(x => x.PageSize).InclusiveBetween(1, EntityConstraints.MaxPageSize)
            .WithMessage(_ => string.Format(ErrorMessage.InvalidPageSize, EntityConstraints.MaxPageSize));
    }
}