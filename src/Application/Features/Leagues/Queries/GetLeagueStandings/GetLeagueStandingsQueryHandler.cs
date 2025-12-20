using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Leagues.Queries.GetLeagueStandings
{
    public class GetLeagueStandingsQueryHandler : IRequestHandler<GetLeagueStandingsQuery, Result<IReadOnlyList<LeagueStandingDto>>>
    {
        private readonly ITipprDbContext _db;

        public GetLeagueStandingsQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<IReadOnlyList<LeagueStandingDto>>> Handle(GetLeagueStandingsQuery request, CancellationToken cancellationToken)
        {
            var leagueExists = await _db.Leagues
                .AnyAsync(l => l.Id == request.LeagueId, cancellationToken);

            if (!leagueExists)
                return Result<IReadOnlyList<LeagueStandingDto>>.NotFound("league not found.", "league.not_found");

            var userIsMember = await _db.LeagueMembers
                .AnyAsync(m => m.LeagueId == request.LeagueId && m.UserId == request.UserId, cancellationToken);

            if (!userIsMember)
                return Result<IReadOnlyList<LeagueStandingDto>>.Forbidden("not a member of this league.", "league.forbidden");

            var standings = await _db.LeagueStandings
                .AsNoTracking()
                .Where(s => s.LeagueId == request.LeagueId)
                .Select(s => new LeagueStandingDto
                {
                    UserId = s.UserId,
                    Username = s.User.Username,
                    AvatarUrl = s.User.AvatarUrl,

                    // Rank is now persisted and guaranteed to be >= 1
                    Rank = s.Rank,
                    PreviousRank = s.PreviousRank,
                    RankChange = s.PreviousRank.HasValue
                        ? s.PreviousRank.Value - s.Rank
                        : null,

                    TotalPoints = s.TotalPoints,
                    MatchPoints = s.MatchPoints,
                    BonusPoints = s.BonusPoints
                })
                .OrderBy(x => x.Rank)
                .ThenBy(x => x.Username) // stable ordering for ties
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<LeagueStandingDto>>.Success(standings);
        }
    }
}
