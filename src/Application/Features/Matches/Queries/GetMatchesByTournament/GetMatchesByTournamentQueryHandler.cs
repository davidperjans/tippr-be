using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Matches.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Matches.Queries.GetMatchesByTournament
{
    public sealed class GetMatchesByTournamentQueryHandler : IRequestHandler<GetMatchesByTournamentQuery, Result<IReadOnlyList<MatchListItemDto>>>
    {
        private readonly ITipprDbContext _db;

        public GetMatchesByTournamentQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<IReadOnlyList<MatchListItemDto>>> Handle(GetMatchesByTournamentQuery request, CancellationToken ct)
        {
            var matches = await _db.Matches
                .AsNoTracking()
                .Where(m => m.TournamentId == request.TournamentId)
                .OrderBy(m => m.MatchDate)
                .Select(m => new MatchListItemDto
                {
                    Id = m.Id,
                    TournamentId = m.TournamentId,

                    HomeTeamId = m.HomeTeamId,
                    HomeTeamName = m.HomeTeam.Name,
                    HomeTeamLogoUrl = m.HomeTeam.LogoUrl,
                    HomeTeamFifaRank = m.HomeTeam.FifaRank,

                    AwayTeamId = m.AwayTeamId,
                    AwayTeamName = m.AwayTeam.Name,
                    AwayTeamLogoUrl = m.AwayTeam.LogoUrl,
                    AwayTeamFifaRank = m.AwayTeam.FifaRank,

                    GroupName = m.HomeTeam.Group != null ? m.HomeTeam.Group.Name : (m.AwayTeam.Group != null ? m.AwayTeam.Group.Name : ""),
                    Venue = m.VenueName!,
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
