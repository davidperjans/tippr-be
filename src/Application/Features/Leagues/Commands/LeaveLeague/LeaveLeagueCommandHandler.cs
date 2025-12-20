using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Leagues.Commands.LeaveLeague
{
    public sealed class LeaveLeagueCommandHandler : IRequestHandler<LeaveLeagueCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;
        private readonly ICurrentUser _currentUser;

        public LeaveLeagueCommandHandler(ITipprDbContext db, ICurrentUser currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<Result<bool>> Handle(LeaveLeagueCommand request, CancellationToken ct)
        {
            var userId = _currentUser.UserId;

            var league = await _db.Leagues
                .Include(l => l.Members)
                .FirstOrDefaultAsync(l => l.Id == request.LeagueId, ct);

            if (league == null)
                return Result<bool>.NotFound("League not found.", "league.not_found");

            if (league.IsGlobal)
                return Result<bool>.BusinessRule("you cannot leave the global league", "league.cannot_leave_global");

            if (league.OwnerId == userId)
                return Result<bool>.Forbidden("League owner cannot leave. Transfer ownership or delete the league.", "league.owner_cannot_leave");

            var member = await _db.LeagueMembers.FirstOrDefaultAsync(m => m.LeagueId == request.LeagueId && m.UserId == userId, ct);
            if (member == null)
                return Result<bool>.NotFound("You are not a member of this league.", "league.not_member");

            _db.LeagueMembers.Remove(member);

            var standing = await _db.LeagueStandings
                .FirstOrDefaultAsync(s => s.LeagueId == request.LeagueId && s.UserId == userId, ct);

            if (standing != null)
                _db.LeagueStandings.Remove(standing);

            var predictions = await _db.Predictions
                .Where(p => p.LeagueId == request.LeagueId && p.UserId == userId)
                .ToListAsync(ct);

            if (predictions.Count > 0)
                _db.Predictions.RemoveRange(predictions);

            var bonusPredictions = await _db.BonusPredictions
                .Where(bp => bp.LeagueId == request.LeagueId && bp.UserId == userId)
                .ToListAsync(ct);

            if (bonusPredictions.Count > 0)
                _db.BonusPredictions.RemoveRange(bonusPredictions);

            await _db.SaveChangesAsync(ct);
            return Result<bool>.Success(true);
        }
    }
}
