using Application.Common;
using MediatR;

namespace Application.Features.Leagues.Commands.LeaveLeague
{
    public sealed record LeaveLeagueCommand(
        Guid LeagueId
    ) : IRequest<Result<bool>>;
}
