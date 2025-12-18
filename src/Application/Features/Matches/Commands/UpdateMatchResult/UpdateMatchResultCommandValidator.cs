using Domain.Enums;
using FluentValidation;

namespace Application.Features.Matches.Commands.UpdateMatchResult
{
    public sealed class UpdateMatchResultCommandValidator : AbstractValidator<UpdateMatchResultCommand>
    {
        public UpdateMatchResultCommandValidator()
        {
            RuleFor(x => x.MatchId).NotEmpty();

            RuleFor(x => x.Status).IsInEnum();

            RuleFor(x => x.HomeScore)
                .GreaterThanOrEqualTo(0)
                .When(x => x.HomeScore.HasValue);

            RuleFor(x => x.AwayScore)
                .GreaterThanOrEqualTo(0)
                .When(x => x.AwayScore.HasValue);

            // Om matchen är FullTime: kräva båda scorer
            When(x => x.Status == MatchStatus.FullTime, () =>
            {
                RuleFor(x => x.HomeScore).NotNull();
                RuleFor(x => x.AwayScore).NotNull();
            });
        }
    }
}
