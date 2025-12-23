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
                team.FlagUrl = request.FlagUrl;

            if (request.GroupName != null)
                team.GroupName = request.GroupName;

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

            var dto = new AdminTeamDto
            {
                Id = team.Id,
                TournamentId = team.TournamentId,
                TournamentName = team.Tournament.Name,
                Name = team.Name,
                Code = team.Code,
                FlagUrl = team.FlagUrl,
                GroupName = team.GroupName,
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
