using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Tournaments.Commands.DeleteTournament
{
    public class DeleteTournamentCommandHandler : IRequestHandler<DeleteTournamentCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;

        public DeleteTournamentCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<bool>> Handle(DeleteTournamentCommand request, CancellationToken cancellationToken)
        {
            var tournament = await _db.Tournaments
                .Include(t => t.Teams)
                .Include(t => t.Matches)
                .Include(t => t.BonusQuestions)
                .Include(t => t.Leagues)
                .FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);

            if (tournament == null)
                return Result<bool>.NotFound("Tournament not found", "admin.tournament_not_found");

            // Check for existing leagues
            if (tournament.Leagues.Any())
                return Result<bool>.BusinessRule("Cannot delete tournament with existing leagues. Delete leagues first.", "admin.tournament_has_leagues");

            // Remove related data
            _db.Matches.RemoveRange(tournament.Matches);
            _db.BonusQuestions.RemoveRange(tournament.BonusQuestions);
            _db.Teams.RemoveRange(tournament.Teams);
            _db.Tournaments.Remove(tournament);

            await _db.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
