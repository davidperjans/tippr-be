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
        public JoinLeagueCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<bool>> Handle(JoinLeagueCommand request, CancellationToken cancellationToken)
        {
            var league = await _db.Leagues
                .Include(l => l.Members)
                .FirstOrDefaultAsync(l => l.Id == request.LeagueId, cancellationToken);

            if (league == null)
                return Result<bool>.Failure("league not found");

            // If league is not public, requires code
            if (!league.IsPublic && !string.Equals(league.InviteCode, request.InviteCode, StringComparison.OrdinalIgnoreCase))
                return Result<bool>.Failure("invalid invite code");

            var alreadyMember = league.Members.Any(m => m.UserId == request.UserId);
            if (alreadyMember)
                return Result<bool>.Success(true);

            if (league.MaxMembers.HasValue && league.Members.Count >= league.MaxMembers.Value)
                return Result<bool>.Failure("league is full");

            _db.LeagueMembers.Add(new Domain.Entities.LeagueMember
            {
                Id = Guid.NewGuid(),
                LeagueId = league.Id,
                UserId = request.UserId,
                JoinedAt = DateTime.UtcNow,
                IsAdmin = false,
                IsMuted = false
            });

            var standingExists = await _db.LeagueStandings
                .AnyAsync(s => s.LeagueId == league.Id && s.UserId == request.UserId, cancellationToken);

            if (!standingExists)
            {
                _db.LeagueStandings.Add(new LeagueStanding
                {
                    Id = Guid.NewGuid(),
                    LeagueId = league.Id,
                    UserId = request.UserId,
                    TotalPoints = 0,
                    MatchPoints = 0,
                    BonusPoints = 0,
                    Rank = 0,
                    PreviousRank = null,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
