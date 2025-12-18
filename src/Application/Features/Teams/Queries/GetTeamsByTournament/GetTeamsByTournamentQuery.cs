using Application.Common;
using Application.Features.Teams.DTOs;
using MediatR;

namespace Application.Features.Teams.Queries.GetTeamsByTournament
{

    public sealed record GetTeamsByTournamentQuery(
        Guid TournamentId
    ) : IRequest<Result<IReadOnlyList<TeamDto>>>;
}
