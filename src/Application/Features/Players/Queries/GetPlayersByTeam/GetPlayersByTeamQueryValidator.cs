using FluentValidation;

namespace Application.Features.Players.Queries.GetPlayersByTeam
{
    public sealed class GetPlayersByTeamQueryValidator : AbstractValidator<GetPlayersByTeamQuery>
    {
        public GetPlayersByTeamQueryValidator()
        {
            RuleFor(x => x.TeamId).NotEmpty();
        }
    }
}
