using Application.Common;
using MediatR;

namespace Application.Features.Leagues.Commands.DeleteLeague
{
    public sealed record DeleteLeagueCommand(
        Guid LeagueId,
        Guid UserId
    ) : IRequest<Result<bool>>;
}
