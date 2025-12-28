using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Users.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UserProfileDto>>
    {
        private readonly ITipprDbContext _db;

        public UpdateProfileCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(u => u.FavoriteTeam)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result<UserProfileDto>.NotFound("User not found", "user.not_found");

            // Update DisplayName if provided
            if (!string.IsNullOrWhiteSpace(request.DisplayName))
                user.DisplayName = request.DisplayName;

            // Update Bio (allow setting to empty string to clear it)
            if (request.Bio != null)
                user.Bio = request.Bio;

            // Update FavoriteTeamId if provided
            if (request.FavoriteTeamId.HasValue)
            {
                // Verify team exists
                var teamExists = await _db.Teams
                    .AnyAsync(t => t.Id == request.FavoriteTeamId.Value, cancellationToken);

                if (!teamExists)
                    return Result<UserProfileDto>.NotFound("Team not found", "user.team_not_found");

                user.FavoriteTeamId = request.FavoriteTeamId.Value;
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            // Get favorite team name if set
            string? favoriteTeamName = null;
            if (user.FavoriteTeamId.HasValue)
            {
                favoriteTeamName = await _db.Teams
                    .Where(t => t.Id == user.FavoriteTeamId.Value)
                    .Select(t => t.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var dto = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                FavoriteTeamId = user.FavoriteTeamId,
                FavoriteTeamName = favoriteTeamName,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return Result<UserProfileDto>.Success(dto);
        }
    }
}
