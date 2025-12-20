namespace Application.Features.Leagues.DTOs
{
    public class LeagueListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid TournamentId { get; set; }
        public Guid? OwnerId { get; set; }
        public string InviteCode { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = false;
        public bool IsGlobal { get; set; } = false;
        public int? MaxMembers { get; set; }
        public string? ImageUrl { get; set; }
        public int MemberCount { get; set; }
        public int MyRank { get; set; }
        public int MyTotalPoints { get; set; }
    }
}
