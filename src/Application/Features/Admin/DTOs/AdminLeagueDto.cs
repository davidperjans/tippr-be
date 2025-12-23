namespace Application.Features.Admin.DTOs
{
    public class AdminLeagueDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public Guid TournamentId { get; init; }
        public string TournamentName { get; init; } = string.Empty;
        public Guid? OwnerId { get; init; }
        public string? OwnerUsername { get; init; }
        public string InviteCode { get; init; } = string.Empty;
        public bool IsPublic { get; init; }
        public bool IsGlobal { get; init; }
        public bool IsSystemCreated { get; init; }
        public int? MaxMembers { get; init; }
        public string? ImageUrl { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public int MemberCount { get; init; }
        public int PredictionCount { get; init; }
    }

    public class AdminLeagueListDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public Guid TournamentId { get; init; }
        public string TournamentName { get; init; } = string.Empty;
        public Guid? OwnerId { get; init; }
        public string? OwnerUsername { get; init; }
        public bool IsPublic { get; init; }
        public bool IsGlobal { get; init; }
        public int? MaxMembers { get; init; }
        public DateTime CreatedAt { get; init; }
        public int MemberCount { get; init; }
    }

    public class AdminLeagueMemberDto
    {
        public Guid Id { get; init; }
        public Guid LeagueId { get; init; }
        public Guid UserId { get; init; }
        public string Username { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? AvatarUrl { get; init; }
        public DateTime JoinedAt { get; init; }
        public bool IsAdmin { get; init; }
        public bool IsMuted { get; init; }
        public int TotalPoints { get; init; }
        public int Rank { get; init; }
    }
}
