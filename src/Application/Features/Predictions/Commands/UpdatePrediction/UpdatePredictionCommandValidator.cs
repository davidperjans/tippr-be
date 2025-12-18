using FluentValidation;

namespace Application.Features.Predictions.Commands.UpdatePrediction
{
    public sealed class UpdatePredictionCommandValidator : AbstractValidator<UpdatePredictionCommand>
    {
        public UpdatePredictionCommandValidator()
        {
            RuleFor(x => x.PredictionId).NotEmpty();
            RuleFor(x => x.HomeScore).GreaterThanOrEqualTo(0);
            RuleFor(x => x.AwayScore).GreaterThanOrEqualTo(0);
        }
    }
}
