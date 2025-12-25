using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Matches.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Matches.Queries.GetMatchesByDate
{
    public sealed class GetMatchesByDateQueryHandler : IRequestHandler<GetMatchesByDateQuery, Result<IReadOnlyList<MatchListItemDto>>>
    {
        private readonly ITipprDbContext _db;

        public GetMatchesByDateQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<IReadOnlyList<MatchListItemDto>>> Handle(GetMatchesByDateQuery request, CancellationToken ct)
        {
            var startUtc = request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endUtc = startUtc.AddDays(1);

            var matches = await _db.Matches
                .AsNoTracking()
                .Where(m => m.MatchDate >= startUtc && m.MatchDate < endUtc)
                .OrderBy(m => m.MatchDate)
                .Select(m => new MatchListItemDto
                {
                    Id = m.Id,
                    TournamentId = m.TournamentId,

                    HomeTeamId = m.HomeTeamId,
                    HomeTeamName = m.HomeTeam.Name,
                    HomeTeamLogoUrl = m.HomeTeam.LogoUrl,

                    AwayTeamId = m.AwayTeamId,
                    AwayTeamName = m.AwayTeam.Name,
                    AwayTeamLogoUrl = m.AwayTeam.LogoUrl,

                    MatchDate = m.MatchDate,
                    Stage = m.Stage,
                    Status = m.Status,
                    HomeScore = m.HomeScore,
                    AwayScore = m.AwayScore
                })
                .ToListAsync(ct);

            return Result<IReadOnlyList<MatchListItemDto>>.Success(matches);
        }
    }
}
