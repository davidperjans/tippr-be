using FluentValidation;

namespace Application.Features.BonusQuestions.Commands.CreateBonusQuestion
{
    public sealed class CreateBonusQuestionCommandValidator : AbstractValidator<CreateBonusQuestionCommand>
    {
        public CreateBonusQuestionCommandValidator()
        {
            RuleFor(x => x.TournamentId)
                .NotEmpty()
                .WithMessage("TournamentId is required");

            RuleFor(x => x.QuestionType)
                .IsInEnum()
                .WithMessage("Invalid question type");

            RuleFor(x => x.Question)
                .NotEmpty()
                .WithMessage("Question is required")
                .MaximumLength(500)
                .WithMessage("Question must not exceed 500 characters");

            RuleFor(x => x.Points)
                .GreaterThan(0)
                .WithMessage("Points must be greater than 0");
        }
    }
}
