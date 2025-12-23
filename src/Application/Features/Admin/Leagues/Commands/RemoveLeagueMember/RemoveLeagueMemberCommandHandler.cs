using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Leagues.Commands.RemoveLeagueMember
{
    public class RemoveLeagueMemberCommandHandler : IRequestHandler<RemoveLeagueMemberCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;
        private readonly IStandingsService _standingsService;

        public RemoveLeagueMemberCommandHandler(ITipprDbContext db, IStandingsService standingsService)
        {
            _db = db;
            _standingsService = standingsService;
        }

        public async Task<Result<bool>> Handle(RemoveLeagueMemberCommand request, CancellationToken cancellationToken)
        {
            var member = await _db.LeagueMembers
                .FirstOrDefaultAsync(lm => lm.LeagueId == request.LeagueId && lm.UserId == request.UserId, cancellationToken);

            if (member == null)
                return Result<bool>.NotFound("Member not found in this league", "admin.member_not_found");

            // Check if user is the owner
            var league = await _db.Leagues
                .FirstOrDefaultAsync(l => l.Id == request.LeagueId, cancellationToken);

            if (league != null && league.OwnerId == request.UserId)
                return Result<bool>.BusinessRule("Cannot remove the league owner", "admin.cannot_remove_owner");

            // Remove member's predictions for this league
            var predictions = await _db.Predictions
                .Where(p => p.LeagueId == request.LeagueId && p.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            _db.Predictions.RemoveRange(predictions);

            // Remove member's bonus predictions for this league
            var bonusPredictions = await _db.BonusPredictions
                .Where(bp => bp.LeagueId == request.LeagueId && bp.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            _db.BonusPredictions.RemoveRange(bonusPredictions);

            // Remove standing
            var standing = await _db.LeagueStandings
                .FirstOrDefaultAsync(ls => ls.LeagueId == request.LeagueId && ls.UserId == request.UserId, cancellationToken);

            if (standing != null)
                _db.LeagueStandings.Remove(standing);

            _db.LeagueMembers.Remove(member);

            await _db.SaveChangesAsync(cancellationToken);
            await _standingsService.RecalculateRanksForLeagueAsync(request.LeagueId, cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
