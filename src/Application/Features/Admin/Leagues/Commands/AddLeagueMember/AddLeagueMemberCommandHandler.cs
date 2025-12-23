using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Leagues.Commands.AddLeagueMember
{
    public class AddLeagueMemberCommandHandler : IRequestHandler<AddLeagueMemberCommand, Result<Guid>>
    {
        private readonly ITipprDbContext _db;
        private readonly IStandingsService _standingsService;

        public AddLeagueMemberCommandHandler(ITipprDbContext db, IStandingsService standingsService)
        {
            _db = db;
            _standingsService = standingsService;
        }

        public async Task<Result<Guid>> Handle(AddLeagueMemberCommand request, CancellationToken cancellationToken)
        {
            var league = await _db.Leagues
                .FirstOrDefaultAsync(l => l.Id == request.LeagueId, cancellationToken);

            if (league == null)
                return Result<Guid>.NotFound("League not found", "admin.league_not_found");

            var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
            if (!userExists)
                return Result<Guid>.NotFound("User not found", "admin.user_not_found");

            var alreadyMember = await _db.LeagueMembers
                .AnyAsync(lm => lm.LeagueId == request.LeagueId && lm.UserId == request.UserId, cancellationToken);

            if (alreadyMember)
                return Result<Guid>.Conflict("User is already a member of this league", "admin.user_already_member");

            // Check max members
            if (league.MaxMembers.HasValue)
            {
                var currentMemberCount = await _db.LeagueMembers
                    .CountAsync(lm => lm.LeagueId == request.LeagueId, cancellationToken);

                if (currentMemberCount >= league.MaxMembers.Value)
                    return Result<Guid>.BusinessRule("League has reached maximum members", "admin.league_full");
            }

            var member = new LeagueMember
            {
                Id = Guid.NewGuid(),
                LeagueId = request.LeagueId,
                UserId = request.UserId,
                JoinedAt = DateTime.UtcNow,
                IsAdmin = request.IsAdmin,
                IsMuted = false
            };

            _db.LeagueMembers.Add(member);

            // Add standing for the new member
            _db.LeagueStandings.Add(new LeagueStanding
            {
                Id = Guid.NewGuid(),
                LeagueId = request.LeagueId,
                UserId = request.UserId,
                TotalPoints = 0,
                MatchPoints = 0,
                BonusPoints = 0,
                Rank = 0,
                PreviousRank = null,
                UpdatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(cancellationToken);
            await _standingsService.RecalculateRanksForLeagueAsync(request.LeagueId, cancellationToken);

            return Result<Guid>.Success(member.Id);
        }
    }
}
