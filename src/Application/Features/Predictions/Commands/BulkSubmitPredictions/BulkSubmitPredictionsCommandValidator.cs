using FluentValidation;

namespace Application.Features.Predictions.Commands.BulkSubmitPredictions
{
    public class BulkSubmitPredictionsCommandValidator : AbstractValidator<BulkSubmitPredictionsCommand>
    {
        public BulkSubmitPredictionsCommandValidator()
        {
            RuleFor(x => x.LeagueId)
                .NotEmpty()
                .WithMessage("LeagueId is required");

            RuleFor(x => x.Predictions)
                .NotEmpty()
                .WithMessage("Predictions list cannot be empty")
                .Must(p => p.Count <= 100)
                .WithMessage("Cannot submit more than 100 predictions at once");

            RuleForEach(x => x.Predictions).ChildRules(prediction =>
            {
                prediction.RuleFor(p => p.MatchId)
                    .NotEmpty()
                    .WithMessage("MatchId is required");

                prediction.RuleFor(p => p.HomeScore)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("HomeScore cannot be negative")
                    .LessThanOrEqualTo(99)
                    .WithMessage("HomeScore cannot exceed 99");

                prediction.RuleFor(p => p.AwayScore)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("AwayScore cannot be negative")
                    .LessThanOrEqualTo(99)
                    .WithMessage("AwayScore cannot exceed 99");
            });
        }
    }
}
