using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Tournaments.Commands.DeactivateTournament
{
    public class DeactivateTournamentCommandHandler : IRequestHandler<DeactivateTournamentCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;

        public DeactivateTournamentCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<bool>> Handle(DeactivateTournamentCommand request, CancellationToken cancellationToken)
        {
            var tournament = await _db.Tournaments
                .FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);

            if (tournament == null)
                return Result<bool>.NotFound("Tournament not found", "admin.tournament_not_found");

            tournament.IsActive = false;
            tournament.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
