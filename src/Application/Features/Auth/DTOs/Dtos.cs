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
        string? DisplayName,
        string? AvatarUrl,
        DateTime LastLoginAt
    );
}
