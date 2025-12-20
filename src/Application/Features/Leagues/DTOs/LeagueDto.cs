namespace Application.Features.Leagues.DTOs
{
    public class LeagueDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid TournamentId { get; set; }
        public Guid? OwnerId { get; set; }
        public string InviteCode { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public bool IsGlobal { get; set; }
        public int? MaxMembers { get; set; }
        public string? ImageUrl { get; set; }

        // Detail-only
        public LeagueSettingsDto Settings { get; set; } = default!;
        public List<LeagueMemberDto> Members { get; set; } = new();

        public int MemberCount { get; set; }

        // "Me-context" (super useful)
        public int MyRank { get; set; }
        public int MyTotalPoints { get; set; }
        public bool IsOwner { get; set; }
    }
}
