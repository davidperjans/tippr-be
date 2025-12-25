using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Matches.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Matches.Queries.GetMatchesByTeam
{
    public sealed class GetMatchesByTeamQueryHandler
        : IRequestHandler<GetMatchesByTeamQuery, Result<IReadOnlyList<MatchListItemDto>>>
    {
        private readonly ITipprDbContext _db;

        public GetMatchesByTeamQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<IReadOnlyList<MatchListItemDto>>> Handle(
            GetMatchesByTeamQuery request,
            CancellationToken ct)
        {
            var teamExists = await _db.Teams.AnyAsync(t => t.Id == request.TeamId, ct);
            if (!teamExists)
                return Result<IReadOnlyList<MatchListItemDto>>.NotFound("Team not found", "team.not_found");

            var matches = await _db.Matches
                .AsNoTracking()
                .Where(m => m.HomeTeamId == request.TeamId || m.AwayTeamId == request.TeamId)
                .OrderBy(m => m.MatchDate)
                .Select(m => new MatchListItemDto
                {
                    Id = m.Id,
                    TournamentId = m.TournamentId,

                    HomeTeamId = m.HomeTeamId,
                    HomeTeamName = m.HomeTeam.DisplayName ?? m.HomeTeam.Name,
                    HomeTeamLogoUrl = m.HomeTeam.LogoUrl,
                    HomeTeamFifaRank = m.HomeTeam.FifaRank,

                    AwayTeamId = m.AwayTeamId,
                    AwayTeamName = m.AwayTeam.DisplayName ?? m.AwayTeam.Name,
                    AwayTeamLogoUrl = m.AwayTeam.LogoUrl,
                    AwayTeamFifaRank = m.AwayTeam.FifaRank,

                    GroupName = m.HomeTeam.Group != null
                        ? m.HomeTeam.Group.Name
                        : (m.AwayTeam.Group != null ? m.AwayTeam.Group.Name : ""),
                    Venue = m.VenueName ?? "",
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
