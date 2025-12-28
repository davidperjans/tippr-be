using Domain.Enums;

namespace Application.Features.Auth.DTOs
{
    public record SyncUserResponse(
        Guid UserId, 
        string Email, 
        bool IsNewUser
    );
    
    public record CurrentUserResponse(
        Guid UserId,
        string Email,
        string Username,
        string? DisplayName,
        string? AvatarUrl,
        string? Bio,
        Guid? FavoriteTeamId,
        string? FavoriteTeamName,
        DateTime LastLoginAt,
        UserRole Role
    );
}
