using Application.Common;
using Application.Features.Matches.DTOs;
using MediatR;

namespace Application.Features.Matches.Queries.GetMatchesByDate
{
    public sealed record GetMatchesByDateQuery(
        DateOnly Date
    ) : IRequest<Result<IReadOnlyList<MatchDto>>>;
}
