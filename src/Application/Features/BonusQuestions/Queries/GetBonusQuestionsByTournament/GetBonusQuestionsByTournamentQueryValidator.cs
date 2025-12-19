using FluentValidation;

namespace Application.Features.BonusQuestions.Queries.GetBonusQuestionsByTournament
{
    public sealed class GetBonusQuestionsByTournamentQueryValidator : AbstractValidator<GetBonusQuestionsByTournamentQuery>
    {
        public GetBonusQuestionsByTournamentQueryValidator()
        {
            RuleFor(x => x.TournamentId)
                .NotEmpty()
                .WithMessage("TournamentId is required");
        }
    }
}
