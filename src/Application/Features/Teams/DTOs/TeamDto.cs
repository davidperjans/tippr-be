namespace Application.Features.Teams.DTOs
{
    public sealed class TeamDto
    {
        public Guid Id { get; init; }
        public Guid TournamentId { get; init; }

        public string Name { get; init; } = string.Empty;
        public string? DisplayName { get; init; }
        public string Code { get; init; } = string.Empty;

        public string? LogoUrl { get; init; }
        public string? GroupName { get; init; }
        public int? FifaRank { get; init; }
        public int? ApiFootballId { get; init; }
    }
}
