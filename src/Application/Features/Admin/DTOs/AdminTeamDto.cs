namespace Application.Features.Admin.DTOs
{
    public class AdminTeamDto
    {
        public Guid Id { get; init; }
        public Guid TournamentId { get; init; }
        public string TournamentName { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string? LogoUrl { get; init; }
        public string? GroupName { get; init; }
        public int? FifaRank { get; init; }
        public decimal? FifaPoints { get; init; }
        public DateTime? FifaRankingUpdatedAt { get; init; }
        public int? ApiFootballId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }

    public class AdminTeamListDto
    {
        public Guid Id { get; init; }
        public Guid TournamentId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string? LogoUrl { get; init; }
        public string? GroupName { get; init; }
        public int? FifaRank { get; init; }
        public int? ApiFootballId { get; init; }
    }
}
