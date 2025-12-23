using Application.Common;
using MediatR;

namespace Application.Features.Admin.Teams.Commands.DeleteTeam
{
    public sealed record DeleteTeamCommand(Guid TeamId) : IRequest<Result<bool>>;
}
