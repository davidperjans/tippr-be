using Application.Common;
using MediatR;

namespace Application.Features.Admin.Tournaments.Commands.DeactivateTournament
{
    public sealed record DeactivateTournamentCommand(Guid TournamentId) : IRequest<Result<bool>>;
}
