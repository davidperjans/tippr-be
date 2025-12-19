using Application.Common;
using Application.Features.Matches.DTOs;
using MediatR;

namespace Application.Features.Matches.Queries.GetMatch
{
    public sealed record GetMatchQuery(
        Guid Id
    ) : IRequest<Result<MatchDetailDto>>;
}
