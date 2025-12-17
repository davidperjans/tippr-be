using Application.Common;
using MediatR;

namespace Application.Features.Leagues.Commands.JoinLeague
{
    public sealed record JoinLeagueCommand(
        Guid LeagueId,
        Guid UserId,
        string InviteCode
    ) : IRequest<Result<bool>>;
}
