using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Leagues.Commands.DeleteAdminLeague
{
    public class DeleteAdminLeagueCommandHandler : IRequestHandler<DeleteAdminLeagueCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;

        public DeleteAdminLeagueCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<bool>> Handle(DeleteAdminLeagueCommand request, CancellationToken cancellationToken)
        {
            var league = await _db.Leagues
                .Include(l => l.Members)
                .Include(l => l.Predictions)
                .Include(l => l.BonusPredictions)
                .Include(l => l.Standings)
                .Include(l => l.ChatMessages)
                .Include(l => l.Settings)
                .FirstOrDefaultAsync(l => l.Id == request.LeagueId, cancellationToken);

            if (league == null)
                return Result<bool>.NotFound("League not found", "admin.league_not_found");

            // Remove all related data
            _db.LeagueMembers.RemoveRange(league.Members);
            _db.Predictions.RemoveRange(league.Predictions);
            _db.BonusPredictions.RemoveRange(league.BonusPredictions);
            _db.LeagueStandings.RemoveRange(league.Standings);
            _db.ChatMessages.RemoveRange(league.ChatMessages);

            if (league.Settings != null)
                _db.LeagueSettings.Remove(league.Settings);

            _db.Leagues.Remove(league);

            await _db.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
