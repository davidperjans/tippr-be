using Application.Features.Teams.DTOs;
using Domain.Enums;

namespace Application.Features.Matches.DTOs
{
    public sealed class MatchDto
    {
        public Guid Id { get; init; }
        public Guid TournamentId { get; init; }
        public Guid HomeTeamId { get; init; }
        public TeamDto? HomeTeam { get; init; }
        public Guid AwayTeamId { get; init; }
        public TeamDto? AwayTeam { get; init; }
        public DateTime MatchDate { get; init; }
        public MatchStage Stage { get; init; }
        public MatchStatus Status { get; init; }
        public int? HomeScore { get; init; }
        public int? AwayScore { get; init; }
        public string? Venue { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
