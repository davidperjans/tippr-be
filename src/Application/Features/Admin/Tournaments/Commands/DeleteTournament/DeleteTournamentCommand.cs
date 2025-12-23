using Application.Common;
using MediatR;

namespace Application.Features.Admin.Tournaments.Commands.DeleteTournament
{
    public sealed record DeleteTournamentCommand(Guid TournamentId) : IRequest<Result<bool>>;
}
