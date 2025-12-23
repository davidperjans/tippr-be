using Application.Common;
using MediatR;

namespace Application.Features.Admin.Leagues.Commands.AddLeagueMember
{
    public sealed record AddLeagueMemberCommand(
        Guid LeagueId,
        Guid UserId,
        bool IsAdmin = false
    ) : IRequest<Result<Guid>>;
}
