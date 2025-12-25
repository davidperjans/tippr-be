namespace Application.Common.Interfaces
{
    public interface IApiFootballClient
    {
        Task<ApiFootballTeamsResult> GetTeamsAsync(int leagueId, int season, CancellationToken ct = default);
        Task<ApiFootballFixturesResult> GetFixturesAsync(int leagueId, int season, CancellationToken ct = default);
        Task<ApiFootballFixturesResult> GetFixturesByIdsAsync(IEnumerable<int> fixtureIds, CancellationToken ct = default);
        Task<ApiFootballLineupsResult> GetLineupsAsync(int fixtureId, CancellationToken ct = default);
        Task<ApiFootballLeagueValidationResult> ValidateLeagueAsync(int leagueId, int season, CancellationToken ct = default);
        Task<ApiFootballStandingsResult> GetStandingsAsync(int leagueId, int season, CancellationToken ct = default);
        Task<ApiFootballSquadResult> GetSquadAsync(int teamId, CancellationToken ct = default);
        Task<string> GetLineupsRawJsonAsync(int fixtureId, CancellationToken ct = default);
        Task<string> GetStatisticsRawJsonAsync(int fixtureId, CancellationToken ct = default);
        Task<string> GetEventsRawJsonAsync(int fixtureId, CancellationToken ct = default);
    }

    // Application-layer result types (decoupled from Infrastructure DTOs)
    public sealed class ApiFootballTeamsResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public List<ApiFootballTeam> Teams { get; init; } = new();
    }

    public sealed class ApiFootballTeam
    {
        public int ApiFootballId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Code { get; init; }
        public int? FoundedYear { get; init; }
        public string? LogoUrl { get; init; }
        public bool IsNational { get; init; }
        public ApiFootballVenue? Venue { get; init; }
    }

    public sealed class ApiFootballVenue
    {
        public int? ApiFootballId { get; init; }
        public string? Name { get; init; }
        public string? Address { get; init; }
        public string? City { get; init; }
        public int? Capacity { get; init; }
        public string? Surface { get; init; }
        public string? ImageUrl { get; init; }
    }

    public sealed class ApiFootballFixturesResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public List<ApiFootballFixture> Fixtures { get; init; } = new();
    }

    public sealed class ApiFootballFixture
    {
        public int ApiFootballId { get; init; }
        public DateTime MatchDateUtc { get; init; }
        public string StatusShort { get; init; } = string.Empty;
        public string StatusLong { get; init; } = string.Empty;
        public int? Elapsed { get; init; }
        public string? Round { get; init; }

        public int HomeTeamApiId { get; init; }
        public string HomeTeamName { get; init; } = string.Empty;
        public int AwayTeamApiId { get; init; }
        public string AwayTeamName { get; init; } = string.Empty;

        public int? HomeScore { get; init; }
        public int? AwayScore { get; init; }

        public ApiFootballVenue? Venue { get; init; }
    }

    public sealed class ApiFootballLineupsResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public string? RawJson { get; init; }
        public List<ApiFootballLineup> Lineups { get; init; } = new();
    }

    public sealed class ApiFootballLineup
    {
        public int TeamApiId { get; init; }
        public string TeamName { get; init; } = string.Empty;
        public string? Formation { get; init; }
        public List<ApiFootballPlayer> StartingXI { get; init; } = new();
        public List<ApiFootballPlayer> Substitutes { get; init; } = new();
        public string? CoachName { get; init; }
    }

    public sealed class ApiFootballPlayer
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int? Number { get; init; }
        public string? Position { get; init; }
    }

    public sealed class ApiFootballLeagueValidationResult
    {
        public bool Success { get; init; }
        public bool IsValid { get; init; }
        public string? ErrorMessage { get; init; }
        public string? LeagueName { get; init; }
        public string? LeagueType { get; init; }
        public string? Country { get; init; }
        public bool HasLineupsSupport { get; init; }
        public bool HasEventsSupport { get; init; }
        public bool HasStatisticsSupport { get; init; }
    }

    public sealed class ApiFootballStandingsResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public List<ApiFootballGroupStanding> GroupStandings { get; init; } = new();
    }

    public sealed class ApiFootballGroupStanding
    {
        public string GroupName { get; init; } = string.Empty;
        public List<ApiFootballTeamStanding> Teams { get; init; } = new();
    }

    public sealed class ApiFootballTeamStanding
    {
        public int TeamApiId { get; init; }
        public string TeamName { get; init; } = string.Empty;
        public int Position { get; init; }
        public int Played { get; init; }
        public int Won { get; init; }
        public int Drawn { get; init; }
        public int Lost { get; init; }
        public int GoalsFor { get; init; }
        public int GoalsAgainst { get; init; }
        public int GoalDifference { get; init; }
        public int Points { get; init; }
        public string? Form { get; init; }
    }

    public sealed class ApiFootballSquadResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public int TeamApiId { get; init; }
        public string TeamName { get; init; } = string.Empty;
        public List<ApiFootballSquadPlayer> Players { get; init; } = new();
    }

    public sealed class ApiFootballSquadPlayer
    {
        public int ApiFootballId { get; init; }
        public string Name { get; init; } = string.Empty;
        public int? Age { get; init; }
        public int? Number { get; init; }
        public string? Position { get; init; }
        public string? PhotoUrl { get; init; }
    }
}
