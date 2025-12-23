using Application.Common;
using MediatR;

namespace Application.Features.Admin.Leagues.Commands.DeleteAdminLeague
{
    public sealed record DeleteAdminLeagueCommand(Guid LeagueId) : IRequest<Result<bool>>;
}
