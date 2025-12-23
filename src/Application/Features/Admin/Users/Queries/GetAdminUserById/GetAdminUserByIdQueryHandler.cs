using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Users.Queries.GetAdminUserById
{
    public class GetAdminUserByIdQueryHandler : IRequestHandler<GetAdminUserByIdQuery, Result<AdminUserDto>>
    {
        private readonly ITipprDbContext _db;

        public GetAdminUserByIdQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<AdminUserDto>> Handle(GetAdminUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .AsNoTracking()
                .Where(u => u.Id == request.UserId)
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    AuthUserId = u.AuthUserId,
                    Username = u.Username,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                    AvatarUrl = u.AvatarUrl,
                    Bio = u.Bio,
                    Role = u.Role,
                    IsBanned = u.IsBanned,
                    FavoriteTeamId = u.FavoriteTeamId,
                    FavoriteTeamName = u.FavoriteTeam != null ? u.FavoriteTeam.Name : null,
                    LastLoginAt = u.LastLoginAt,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    LeagueCount = u.LeagueMemberships.Count,
                    OwnedLeagueCount = u.OwnedLeagues.Count,
                    PredictionCount = u.Predictions.Count
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
                return Result<AdminUserDto>.NotFound("User not found", "admin.user_not_found");

            return Result<AdminUserDto>.Success(user);
        }
    }
}
