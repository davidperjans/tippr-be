using Application.Common;
using Domain.Enums;
using MediatR;

namespace Application.Features.Matches.Commands.UpdateMatchResult
{
    public sealed record UpdateMatchResultCommand(
        Guid MatchId,
        int? HomeScore,
        int? AwayScore,
        MatchStatus Status
    ) : IRequest<Result<bool>>;
}
