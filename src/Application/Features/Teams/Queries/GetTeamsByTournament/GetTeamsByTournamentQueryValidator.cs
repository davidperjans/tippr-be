using FluentValidation;

namespace Application.Features.Teams.Queries.GetTeamsByTournament
{
    public sealed class GetTeamsByTournamentQueryValidator : AbstractValidator<GetTeamsByTournamentQuery>
    {
        public GetTeamsByTournamentQueryValidator()
        {
            RuleFor(x => x.TournamentId)
                .NotEmpty()
                .WithMessage("TournamentId is required");
        }
    }
}
