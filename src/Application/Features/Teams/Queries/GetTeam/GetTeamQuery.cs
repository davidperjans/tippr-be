using Application.Common;
using Application.Features.Teams.DTOs;
using MediatR;

namespace Application.Features.Teams.Queries.GetTeam
{
    public sealed record GetTeamQuery(
        Guid Id
    ) : IRequest<Result<TeamDto>>;
}
