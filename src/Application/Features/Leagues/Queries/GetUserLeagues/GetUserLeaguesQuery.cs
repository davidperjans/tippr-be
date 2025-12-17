using Application.Common;
using Application.Features.Leagues.DTOs;
using MediatR;

namespace Application.Features.Leagues.Queries.GetUserLeagues
{
    public sealed record GetUserLeaguesQuery(
        Guid UserId
    ) : IRequest<Result<IReadOnlyList<LeagueDto>>>;
}
