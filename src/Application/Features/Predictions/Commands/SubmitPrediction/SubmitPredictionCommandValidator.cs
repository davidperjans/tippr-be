using FluentValidation;

namespace Application.Features.Predictions.Commands.SubmitPrediction
{
    public sealed class SubmitPredictionCommandValidator : AbstractValidator<SubmitPredictionCommand>
    {
        public SubmitPredictionCommandValidator()
        {
            RuleFor(x => x.LeagueId).NotEmpty();
            RuleFor(x => x.MatchId).NotEmpty();
            RuleFor(x => x.HomeScore).GreaterThanOrEqualTo(0);
            RuleFor(x => x.AwayScore).GreaterThanOrEqualTo(0);
        }
    }
}
