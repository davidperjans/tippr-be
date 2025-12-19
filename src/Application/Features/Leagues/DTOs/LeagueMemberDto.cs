using Application.Features.Auth.DTOs;

namespace Application.Features.Leagues.DTOs
{
    public sealed class LeagueMemberDto
    {
        public Guid UserId { get; init; }
        public string Username { get; init; } = string.Empty;
        public string? AvatarUrl { get; init; }
        public DateTime JoinedAt { get; init; }
        public bool IsAdmin { get; init; }
        public bool IsMuted { get; init; }
    }
}
