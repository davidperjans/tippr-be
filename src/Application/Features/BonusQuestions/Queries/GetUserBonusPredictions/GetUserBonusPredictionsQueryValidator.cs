using FluentValidation;

namespace Application.Features.BonusQuestions.Queries.GetUserBonusPredictions
{
    public sealed class GetUserBonusPredictionsQueryValidator : AbstractValidator<GetUserBonusPredictionsQuery>
    {
        public GetUserBonusPredictionsQueryValidator()
        {
            RuleFor(x => x.LeagueId)
                .NotEmpty()
                .WithMessage("LeagueId is required");
        }
    }
}
