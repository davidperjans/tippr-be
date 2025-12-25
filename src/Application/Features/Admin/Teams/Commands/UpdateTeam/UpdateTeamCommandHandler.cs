using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Teams.Commands.UpdateTeam
{
    public class UpdateTeamCommandHandler : IRequestHandler<UpdateTeamCommand, Result<AdminTeamDto>>
    {
        private readonly ITipprDbContext _db;

        public UpdateTeamCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<AdminTeamDto>> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
        {
            var team = await _db.Teams
                .Include(t => t.Tournament)
                .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken);

            if (team == null)
                return Result<AdminTeamDto>.NotFound("Team not found", "admin.team_not_found");

            if (!string.IsNullOrWhiteSpace(request.Name))
                team.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Code))
                team.Code = request.Code;

            if (request.FlagUrl != null)
                team.LogoUrl = request.FlagUrl;

            // GroupName is now managed via Group entity and standings sync
            // Use SyncGroupStandings endpoint to assign teams to groups

            if (request.FifaRank.HasValue)
            {
                team.FifaRank = request.FifaRank.Value;
                team.FifaRankingUpdatedAt = DateTime.UtcNow;
            }

            if (request.FifaPoints.HasValue)
                team.FifaPoints = request.FifaPoints.Value;

            if (request.ApiFootballId.HasValue)
                team.ApiFootballId = request.ApiFootballId.Value;

            team.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            // Load group name if team is assigned to a group
            string? groupName = null;
            if (team.GroupId.HasValue)
            {
                groupName = await _db.Groups
                    .Where(g => g.Id == team.GroupId.Value)
                    .Select(g => g.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var dto = new AdminTeamDto
            {
                Id = team.Id,
                TournamentId = team.TournamentId,
                TournamentName = team.Tournament.Name,
                Name = team.Name,
                Code = team.Code ?? string.Empty,
                LogoUrl = team.LogoUrl,
                GroupName = groupName,
                FifaRank = team.FifaRank,
                FifaPoints = team.FifaPoints,
                FifaRankingUpdatedAt = team.FifaRankingUpdatedAt,
                ApiFootballId = team.ApiFootballId,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt
            };

            return Result<AdminTeamDto>.Success(dto);
        }
    }
}
