using Application.Common;
using Application.Features.Leagues.DTOs;
using MediatR;

namespace Application.Features.Leagues.Queries.GetLeagueStandings
{
    public sealed record GetLeagueStandingsQuery(
        Guid LeagueId,
        Guid UserId
    ) : IRequest<Result<IReadOnlyList<LeagueStandingDto>>>;
}
