using Application.Common;
using Application.Features.Leagues.DTOs;
using MediatR;

namespace Application.Features.Leagues.Queries.GetLeague
{
    public sealed record GetLeagueQuery(
        Guid LeagueId,
        Guid UserId
    ) : IRequest<Result<LeagueDto>>;
}
