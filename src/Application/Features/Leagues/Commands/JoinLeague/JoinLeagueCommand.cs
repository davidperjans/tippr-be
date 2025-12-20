using Application.Common;
using MediatR;

namespace Application.Features.Leagues.Commands.JoinLeague
{
    public sealed record JoinLeagueCommand(
        Guid LeagueId,
        string? InviteCode
    ) : IRequest<Result<bool>>;
}
