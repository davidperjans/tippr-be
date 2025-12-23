using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Tournaments.Commands.ActivateTournament
{
    public class ActivateTournamentCommandHandler : IRequestHandler<ActivateTournamentCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;

        public ActivateTournamentCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<bool>> Handle(ActivateTournamentCommand request, CancellationToken cancellationToken)
        {
            var tournament = await _db.Tournaments
                .FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);

            if (tournament == null)
                return Result<bool>.NotFound("Tournament not found", "admin.tournament_not_found");

            tournament.IsActive = true;
            tournament.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
