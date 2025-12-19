using FluentValidation;

namespace Application.Features.BonusQuestions.Commands.SubmitBonusPrediction
{
    public sealed class SubmitBonusPredictionCommandValidator : AbstractValidator<SubmitBonusPredictionCommand>
    {
        public SubmitBonusPredictionCommandValidator()
        {
            RuleFor(x => x.LeagueId)
                .NotEmpty()
                .WithMessage("LeagueId is required");

            RuleFor(x => x.BonusQuestionId)
                .NotEmpty()
                .WithMessage("BonusQuestionId is required");

            RuleFor(x => x.AnswerText)
                .MaximumLength(500)
                .WithMessage("Answer text must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.AnswerText));

            RuleFor(x => x)
                .Must(x => x.AnswerTeamId.HasValue || !string.IsNullOrWhiteSpace(x.AnswerText))
                .WithMessage("An answer must be provided (either team or text)");
        }
    }
}
