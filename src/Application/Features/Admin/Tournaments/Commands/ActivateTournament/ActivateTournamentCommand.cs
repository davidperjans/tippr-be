using Application.Common;
using MediatR;

namespace Application.Features.Admin.Tournaments.Commands.ActivateTournament
{
    public sealed record ActivateTournamentCommand(Guid TournamentId) : IRequest<Result<bool>>;
}
