using Application.Common;
using Application.Features.Matches.DTOs;
using MediatR;

namespace Application.Features.Matches.Queries.GetMatchesByTeam
{
    public sealed record GetMatchesByTeamQuery(Guid TeamId) : IRequest<Result<IReadOnlyList<MatchListItemDto>>>;
}
