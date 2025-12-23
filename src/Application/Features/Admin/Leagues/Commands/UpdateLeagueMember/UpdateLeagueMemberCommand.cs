using Application.Common;
using MediatR;

namespace Application.Features.Admin.Leagues.Commands.UpdateLeagueMember
{
    public sealed record UpdateLeagueMemberCommand(
        Guid LeagueId,
        Guid UserId,
        bool? IsAdmin,
        bool? IsMuted
    ) : IRequest<Result<bool>>;
}
