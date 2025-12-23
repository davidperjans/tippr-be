using Domain.Enums;

namespace Application.Features.Admin.DTOs
{
    public class AdminTournamentDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int Year { get; init; }
        public TournamentType Type { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public string? LogoUrl { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public int TeamCount { get; init; }
        public int MatchCount { get; init; }
        public int LeagueCount { get; init; }
        public int BonusQuestionCount { get; init; }
    }
}
