namespace Application.Features.Users.DTOs
{
    public sealed class UserProfileDto
    {
        public Guid Id { get; init; }
        public string Username { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string? AvatarUrl { get; init; }
        public string? Bio { get; init; }
        public Guid? FavoriteTeamId { get; init; }
        public string? FavoriteTeamName { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
