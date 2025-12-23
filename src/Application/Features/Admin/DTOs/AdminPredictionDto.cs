namespace Application.Features.Admin.DTOs
{
    public class AdminPredictionDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Username { get; init; } = string.Empty;
        public string UserDisplayName { get; init; } = string.Empty;
        public Guid MatchId { get; init; }
        public string MatchDescription { get; init; } = string.Empty;
        public Guid LeagueId { get; init; }
        public string LeagueName { get; init; } = string.Empty;
        public int HomeScore { get; init; }
        public int AwayScore { get; init; }
        public int? ActualHomeScore { get; init; }
        public int? ActualAwayScore { get; init; }
        public int PointsEarned { get; init; }
        public bool IsScored { get; init; }
        public int? ScoredResultVersion { get; init; }
        public DateTime? ScoredAt { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }

    public class AdminPredictionListDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Username { get; init; } = string.Empty;
        public Guid MatchId { get; init; }
        public string MatchDescription { get; init; } = string.Empty;
        public Guid LeagueId { get; init; }
        public string LeagueName { get; init; } = string.Empty;
        public int HomeScore { get; init; }
        public int AwayScore { get; init; }
        public int PointsEarned { get; init; }
        public bool IsScored { get; init; }
    }
}
