using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Leagues.Queries.GetAdminLeagueById
{
    public class GetAdminLeagueByIdQueryHandler : IRequestHandler<GetAdminLeagueByIdQuery, Result<AdminLeagueDto>>
    {
        private readonly ITipprDbContext _db;

        public GetAdminLeagueByIdQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<AdminLeagueDto>> Handle(GetAdminLeagueByIdQuery request, CancellationToken cancellationToken)
        {
            var league = await _db.Leagues
                .AsNoTracking()
                .Where(l => l.Id == request.LeagueId)
                .Select(l => new AdminLeagueDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    TournamentId = l.TournamentId,
                    TournamentName = l.Tournament.Name,
                    OwnerId = l.OwnerId,
                    OwnerUsername = l.Owner != null ? l.Owner.Username : null,
                    InviteCode = l.InviteCode,
                    IsPublic = l.IsPublic,
                    IsGlobal = l.IsGlobal,
                    IsSystemCreated = l.IsSystemCreated,
                    MaxMembers = l.MaxMembers,
                    ImageUrl = l.ImageUrl,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt,
                    MemberCount = l.Members.Count,
                    PredictionCount = l.Predictions.Count
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (league == null)
                return Result<AdminLeagueDto>.NotFound("League not found", "admin.league_not_found");

            return Result<AdminLeagueDto>.Success(league);
        }
    }
}
