using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Users.Commands.UpdateAdminUser
{
    public class UpdateAdminUserCommandHandler : IRequestHandler<UpdateAdminUserCommand, Result<AdminUserDto>>
    {
        private readonly ITipprDbContext _db;

        public UpdateAdminUserCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<AdminUserDto>> Handle(UpdateAdminUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(u => u.FavoriteTeam)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result<AdminUserDto>.NotFound("User not found", "admin.user_not_found");

            // Check for username uniqueness if updating
            if (!string.IsNullOrWhiteSpace(request.Username) && request.Username != user.Username)
            {
                var usernameExists = await _db.Users
                    .AnyAsync(u => u.Username == request.Username && u.Id != request.UserId, cancellationToken);

                if (usernameExists)
                    return Result<AdminUserDto>.Conflict("Username already taken", "admin.username_taken");

                user.Username = request.Username;
            }

            // Check for email uniqueness if updating
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                var emailExists = await _db.Users
                    .AnyAsync(u => u.Email == request.Email && u.Id != request.UserId, cancellationToken);

                if (emailExists)
                    return Result<AdminUserDto>.Conflict("Email already taken", "admin.email_taken");

                user.Email = request.Email;
            }

            if (!string.IsNullOrWhiteSpace(request.DisplayName))
                user.DisplayName = request.DisplayName;

            if (request.Bio != null)
                user.Bio = request.Bio;

            if (request.AvatarUrl != null)
                user.AvatarUrl = request.AvatarUrl;

            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            var dto = new AdminUserDto
            {
                Id = user.Id,
                AuthUserId = user.AuthUserId,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                Role = user.Role,
                IsBanned = user.IsBanned,
                FavoriteTeamId = user.FavoriteTeamId,
                FavoriteTeamName = user.FavoriteTeam?.Name,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LeagueCount = await _db.LeagueMembers.CountAsync(lm => lm.UserId == user.Id, cancellationToken),
                OwnedLeagueCount = await _db.Leagues.CountAsync(l => l.OwnerId == user.Id, cancellationToken),
                PredictionCount = await _db.Predictions.CountAsync(p => p.UserId == user.Id, cancellationToken)
            };

            return Result<AdminUserDto>.Success(dto);
        }
    }
}
