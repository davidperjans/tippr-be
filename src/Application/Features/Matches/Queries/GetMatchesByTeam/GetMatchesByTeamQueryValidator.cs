using FluentValidation;

namespace Application.Features.Matches.Queries.GetMatchesByTeam
{
    public sealed class GetMatchesByTeamQueryValidator : AbstractValidator<GetMatchesByTeamQuery>
    {
        public GetMatchesByTeamQueryValidator()
        {
            RuleFor(x => x.TeamId).NotEmpty();
        }
    }
}
