using Application.Common;
using MediatR;

namespace Application.Features.Admin.Leagues.Commands.RemoveLeagueMember
{
    public sealed record RemoveLeagueMemberCommand(
        Guid LeagueId,
        Guid UserId
    ) : IRequest<Result<bool>>;
}
