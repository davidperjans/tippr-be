using Application.Common;
using Application.Features.Players.DTOs;
using MediatR;

namespace Application.Features.Players.Queries.GetPlayer
{
    public sealed record GetPlayerQuery(Guid Id) : IRequest<Result<PlayerWithTeamDto>>;
}
