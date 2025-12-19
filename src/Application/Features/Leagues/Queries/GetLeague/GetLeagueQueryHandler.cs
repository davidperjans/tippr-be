using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.DTOs;
using Application.Features.Tournaments.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Leagues.Queries.GetLeague
{
    public class GetLeagueQueryHandler : IRequestHandler<GetLeagueQuery, Result<LeagueDto>>
    {
        private readonly ITipprDbContext _db;
        public GetLeagueQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }
        public async Task<Result<LeagueDto>> Handle(GetLeagueQuery request, CancellationToken cancellationToken)
        {
            var leagueId = request.LeagueId;
            var userId = request.UserId;

            var dto = await _db.Leagues
                .AsNoTracking()
                .Where(l => l.Id == leagueId)
                .Select(l => new LeagueDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    TournamentId = l.TournamentId,
                    OwnerId = l.OwnerId,
                    InviteCode = l.InviteCode,
                    IsPublic = l.IsPublic,
                    IsGlobal = l.IsGlobal,
                    MaxMembers = l.MaxMembers,
                    ImageUrl = l.ImageUrl,

                    // --- detail-only (om du lägger till dessa properties i LeagueDto) ---
                    MemberCount = l.Members.Count(),
                    IsOwner = l.OwnerId == userId,

                    MyRank = l.Standings
                        .Where(s => s.UserId == userId)
                        .Select(s => (int?)s.Rank)
                        .FirstOrDefault() ?? 0,

                    MyTotalPoints = l.Standings
                        .Where(s => s.UserId == userId)
                        .Select(s => (int?)s.TotalPoints)
                        .FirstOrDefault() ?? 0,

                    Members = l.Members
                        .OrderBy(m => m.JoinedAt)
                        .Select(m => new LeagueMemberDto
                        {
                            UserId = m.UserId,
                            Username = m.User.Username,   // kräver navigation LeagueMember -> User
                            AvatarUrl = m.User.AvatarUrl, // om du har den
                            JoinedAt = m.JoinedAt,
                            IsAdmin = m.IsAdmin
                        })
                        .ToList(),

                    Settings = new LeagueSettingsDto
                    {
                        PredictionMode = l.Settings.PredictionMode.ToString(),
                        DeadlineMinutes = l.Settings.DeadlineMinutes,
                        PointsCorrectScore = l.Settings.PointsCorrectScore,
                        PointsCorrectOutcome = l.Settings.PointsCorrectOutcome,
                        PointsCorrectGoals = l.Settings.PointsCorrectGoals,
                        PointsRoundOf16Team = l.Settings.PointsRoundOf16Team,
                        PointsQuarterFinalTeam = l.Settings.PointsQuarterFinalTeam,
                        PointsSemiFinalTeam = l.Settings.PointsSemiFinalTeam,
                        PointsFinalTeam = l.Settings.PointsFinalTeam,
                        PointsTopScorer = l.Settings.PointsTopScorer,
                        PointsWinner = l.Settings.PointsWinner,
                        PointsMostGoalsGroup = l.Settings.PointsMostGoalsGroup,
                        PointsMostConcededGroup = l.Settings.PointsMostConcededGroup,
                        AllowLateEdits = l.Settings.AllowLateEdits
                    }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (dto == null)
                return Result<LeagueDto>.NotFound("league not found", "league.not_found");

            return Result<LeagueDto>.Success(dto);
        }
    }
}
