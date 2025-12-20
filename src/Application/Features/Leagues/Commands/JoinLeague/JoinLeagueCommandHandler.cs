using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Leagues.Commands.JoinLeague
{
    public class JoinLeagueCommandHandler : IRequestHandler<JoinLeagueCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;
        private readonly ICurrentUser _currentUser;
        private readonly IStandingsService _standingsService;
        public JoinLeagueCommandHandler(ITipprDbContext db, ICurrentUser currentUser, IStandingsService standingsService)
        {
            _db = db;
            _currentUser = currentUser;
            _standingsService = standingsService;
        }

        public async Task<Result<bool>> Handle(JoinLeagueCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            var league = await _db.Leagues
                .Include(l => l.Members)
                .FirstOrDefaultAsync(l => l.Id == request.LeagueId, cancellationToken);

            if (league == null)
                return Result<bool>.NotFound("league not found", "league.not_found");

            // If not public AND not global => requires invite code
            if (!league.IsPublic && !league.IsGlobal)
            {
                if (string.IsNullOrWhiteSpace(request.InviteCode) ||
                    !string.Equals(league.InviteCode, request.InviteCode, StringComparison.OrdinalIgnoreCase))
                {
                    return Result<bool>.BusinessRule("invalid invite code", "league.invalid_invite_code");
                }
            }

            var alreadyMember = await _db.LeagueMembers.AnyAsync(m => m.LeagueId == league.Id && m.UserId == userId, cancellationToken);
            if (alreadyMember)
                return Result<bool>.Success(true);

            if (league.MaxMembers.HasValue)
            {
                var memberCount = await _db.LeagueMembers
                    .CountAsync(m => m.LeagueId == league.Id, cancellationToken);

                if (memberCount >= league.MaxMembers.Value)
                    return Result<bool>.BusinessRule("league is full", "league.full");
            }

            _db.LeagueMembers.Add(new LeagueMember
            {
                Id = Guid.NewGuid(),
                LeagueId = league.Id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                IsAdmin = false,
                IsMuted = false
            });

            var standingExists = await _db.LeagueStandings
                .AnyAsync(s => s.LeagueId == league.Id && s.UserId == userId, cancellationToken);

            if (!standingExists)
            {
                _db.LeagueStandings.Add(new LeagueStanding
                {
                    Id = Guid.NewGuid(),
                    LeagueId = league.Id,
                    UserId = userId,
                    TotalPoints = 0,
                    MatchPoints = 0,
                    BonusPoints = 0,
                    Rank = 1,
                    PreviousRank = null,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                await _standingsService.RecalculateRanksForLeagueAsync(league.Id, cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (DbUpdateException)
            {
                // If unique constraint triggers due to race condition, treat as success (idempotent).
                var existsNow = await _db.LeagueMembers
                    .AnyAsync(m => m.LeagueId == league.Id && m.UserId == userId, cancellationToken);

                if (existsNow)
                    return Result<bool>.Success(true);

                throw;
            }
        }
    }
}
