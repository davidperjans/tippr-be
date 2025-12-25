using Application.Common;
using Application.Features.Players.DTOs;
using MediatR;

namespace Application.Features.Players.Queries.GetPlayersByTeam
{
    public sealed record GetPlayersByTeamQuery(Guid TeamId) : IRequest<Result<IReadOnlyList<PlayerDto>>>;
}
