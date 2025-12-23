using Domain.Enums;

namespace Application.Features.Admin.DTOs
{
    public class AdminMatchDto
    {
        public Guid Id { get; init; }
        public Guid TournamentId { get; init; }
        public string TournamentName { get; init; } = string.Empty;
        public Guid HomeTeamId { get; init; }
        public string HomeTeamName { get; init; } = string.Empty;
        public string HomeTeamCode { get; init; } = string.Empty;
        public string? HomeTeamLogoUrl { get; init; }
        public Guid AwayTeamId { get; init; }
        public string AwayTeamName { get; init; } = string.Empty;
        public string AwayTeamCode { get; init; } = string.Empty;
        public string? AwayTeamLogoUrl { get; init; }
        public DateTime MatchDate { get; init; }
        public MatchStage Stage { get; init; }
        public int? HomeScore { get; init; }
        public int? AwayScore { get; init; }
        public MatchStatus Status { get; init; }
        public string? Venue { get; init; }
        public int? ApiFootballId { get; init; }
        public int ResultVersion { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public int PredictionCount { get; init; }
    }

    public class AdminMatchListDto
    {
        public Guid Id { get; init; }
        public Guid TournamentId { get; init; }
        public Guid HomeTeamId { get; init; }
        public string HomeTeamName { get; init; } = string.Empty;
        public string HomeTeamCode { get; init; } = string.Empty;
        public Guid AwayTeamId { get; init; }
        public string AwayTeamName { get; init; } = string.Empty;
        public string AwayTeamCode { get; init; } = string.Empty;
        public DateTime MatchDate { get; init; }
        public MatchStage Stage { get; init; }
        public int? HomeScore { get; init; }
        public int? AwayScore { get; init; }
        public MatchStatus Status { get; init; }
    }
}
