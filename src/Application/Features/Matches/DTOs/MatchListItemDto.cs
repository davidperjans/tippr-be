using Domain.Enums;

namespace Application.Features.Matches.DTOs
{
    public sealed class MatchListItemDto
    {
        public Guid Id { get; init; }
        public Guid TournamentId { get; init; }

        public Guid HomeTeamId { get; init; }
        public string HomeTeamName { get; init; } = string.Empty;
        public string? HomeTeamLogoUrl { get; init; }
        public int? HomeTeamFifaRank { get; init; }

        public Guid AwayTeamId { get; init; }
        public string AwayTeamName { get; init; } = string.Empty;
        public string? AwayTeamLogoUrl { get; init; }
        public int? AwayTeamFifaRank { get; init; }

        public string GroupName { get; init; } = string.Empty;
        public string Venue { get; init; } = string.Empty;
        public DateTime MatchDate { get; init; }
        public MatchStage Stage { get; init; }
        public MatchStatus Status { get; init; }

        public int? HomeScore { get; init; }
        public int? AwayScore { get; init; }
    }
}
