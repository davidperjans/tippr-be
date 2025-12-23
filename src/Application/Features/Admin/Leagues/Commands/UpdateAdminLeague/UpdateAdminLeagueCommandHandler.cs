using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Leagues.Commands.UpdateAdminLeague
{
    public class UpdateAdminLeagueCommandHandler : IRequestHandler<UpdateAdminLeagueCommand, Result<AdminLeagueDto>>
    {
        private readonly ITipprDbContext _db;

        public UpdateAdminLeagueCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<AdminLeagueDto>> Handle(UpdateAdminLeagueCommand request, CancellationToken cancellationToken)
        {
            var league = await _db.Leagues
                .Include(l => l.Tournament)
                .Include(l => l.Owner)
                .FirstOrDefaultAsync(l => l.Id == request.LeagueId, cancellationToken);

            if (league == null)
                return Result<AdminLeagueDto>.NotFound("League not found", "admin.league_not_found");

            if (!string.IsNullOrWhiteSpace(request.Name))
                league.Name = request.Name;

            if (request.Description != null)
                league.Description = request.Description;

            if (request.IsPublic.HasValue)
                league.IsPublic = request.IsPublic.Value;

            if (request.IsGlobal.HasValue)
                league.IsGlobal = request.IsGlobal.Value;

            if (request.MaxMembers.HasValue)
                league.MaxMembers = request.MaxMembers.Value;

            if (request.ImageUrl != null)
                league.ImageUrl = request.ImageUrl;

            league.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            var dto = new AdminLeagueDto
            {
                Id = league.Id,
                Name = league.Name,
                Description = league.Description,
                TournamentId = league.TournamentId,
                TournamentName = league.Tournament.Name,
                OwnerId = league.OwnerId,
                OwnerUsername = league.Owner?.Username,
                InviteCode = league.InviteCode,
                IsPublic = league.IsPublic,
                IsGlobal = league.IsGlobal,
                IsSystemCreated = league.IsSystemCreated,
                MaxMembers = league.MaxMembers,
                ImageUrl = league.ImageUrl,
                CreatedAt = league.CreatedAt,
                UpdatedAt = league.UpdatedAt,
                MemberCount = await _db.LeagueMembers.CountAsync(lm => lm.LeagueId == league.Id, cancellationToken),
                PredictionCount = await _db.Predictions.CountAsync(p => p.LeagueId == league.Id, cancellationToken)
            };

            return Result<AdminLeagueDto>.Success(dto);
        }
    }
}
