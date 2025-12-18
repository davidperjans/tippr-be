using Application.Features.Auth.DTOs;

namespace Application.Features.Leagues.DTOs
{
    public sealed class LeagueMemberDto
    {
        public Guid Id { get; init; }
        public Guid LeagueId { get; init; }
        public Guid UserId { get; init; }
        public UserDto User { get; init; } = null!;
        public DateTime JoinedAt { get; init; }
        public bool IsAdmin { get; init; }
        public bool IsMuted { get; init; }
    }
}
