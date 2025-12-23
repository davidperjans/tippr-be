using Application.Common;
using MediatR;

namespace Application.Features.Admin.Leagues.Commands.RegenerateInviteCode
{
    public sealed record RegenerateInviteCodeCommand(Guid LeagueId) : IRequest<Result<string>>;
}
