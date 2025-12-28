using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth.Queries.GetMe
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserResponse>>
    {
        private readonly ITipprDbContext _db;
        private readonly IAuthService _authService;

        public GetCurrentUserQueryHandler(ITipprDbContext db, IAuthService authService)
        {
            _db = db;
            _authService = authService;
        }

        public async Task<Result<CurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(u => u.FavoriteTeam)
                .FirstOrDefaultAsync(u => u.AuthUserId == request.UserId, cancellationToken);

            if (user == null)
                return Result<CurrentUserResponse>.NotFound("user not synced", "user.not_synced");

            await _authService.UpdateLastLoginAsync(user.Id, cancellationToken);

            var resultDto = new CurrentUserResponse(
                user.Id,
                user.Email,
                user.Username,
                user.DisplayName,
                user.AvatarUrl,
                user.Bio,
                user.FavoriteTeamId,
                user.FavoriteTeam?.Name,
                user.LastLoginAt ?? user.CreatedAt,
                user.Role
            );

            return Result<CurrentUserResponse>.Success(resultDto);
        }
    }
}
