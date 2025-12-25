using System.Text.Json.Serialization;

namespace Infrastructure.External.ApiFootball.DTOs
{
    // Base response wrapper
    public class ApiFootballResponse<T>
    {
        [JsonPropertyName("get")]
        public string Get { get; set; } = string.Empty;

        [JsonPropertyName("parameters")]
        public Dictionary<string, string> Parameters { get; set; } = new();

        [JsonPropertyName("errors")]
        public object? Errors { get; set; }

        [JsonPropertyName("results")]
        public int Results { get; set; }

        [JsonPropertyName("paging")]
        public PagingInfo Paging { get; set; } = new();

        [JsonPropertyName("response")]
        public List<T> Response { get; set; } = new();
    }

    public sealed class PagingInfo
    {
        [JsonPropertyName("current")]
        public int Current { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    // Teams endpoint response
    public sealed class TeamResponse
    {
        [JsonPropertyName("team")]
        public TeamInfo Team { get; set; } = new();

        [JsonPropertyName("venue")]
        public VenueInfo? Venue { get; set; }
    }

    public sealed class TeamInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("founded")]
        public int? Founded { get; set; }

        [JsonPropertyName("national")]
        public bool National { get; set; }

        [JsonPropertyName("logo")]
        public string? Logo { get; set; }
    }

    public sealed class VenueInfo
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("capacity")]
        public int? Capacity { get; set; }

        [JsonPropertyName("surface")]
        public string? Surface { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }
    }

    // Fixtures endpoint response
    public sealed class FixtureResponse
    {
        [JsonPropertyName("fixture")]
        public FixtureInfo Fixture { get; set; } = new();

        [JsonPropertyName("league")]
        public LeagueInfo League { get; set; } = new();

        [JsonPropertyName("teams")]
        public TeamsInfo Teams { get; set; } = new();

        [JsonPropertyName("goals")]
        public GoalsInfo Goals { get; set; } = new();

        [JsonPropertyName("score")]
        public ScoreInfo Score { get; set; } = new();
    }

    public sealed class FixtureInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("referee")]
        public string? Referee { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; } = "UTC";

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("periods")]
        public PeriodsInfo? Periods { get; set; }

        [JsonPropertyName("venue")]
        public VenueInfo? Venue { get; set; }

        [JsonPropertyName("status")]
        public StatusInfo Status { get; set; } = new();
    }

    public sealed class PeriodsInfo
    {
        [JsonPropertyName("first")]
        public long? First { get; set; }

        [JsonPropertyName("second")]
        public long? Second { get; set; }
    }

    public sealed class StatusInfo
    {
        [JsonPropertyName("long")]
        public string Long { get; set; } = string.Empty;

        [JsonPropertyName("short")]
        public string Short { get; set; } = string.Empty;

        [JsonPropertyName("elapsed")]
        public int? Elapsed { get; set; }
    }

    public sealed class LeagueInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("logo")]
        public string? Logo { get; set; }

        [JsonPropertyName("flag")]
        public string? Flag { get; set; }

        [JsonPropertyName("season")]
        public int Season { get; set; }

        [JsonPropertyName("round")]
        public string? Round { get; set; }
    }

    public sealed class TeamsInfo
    {
        [JsonPropertyName("home")]
        public TeamBasicInfo Home { get; set; } = new();

        [JsonPropertyName("away")]
        public TeamBasicInfo Away { get; set; } = new();
    }

    public sealed class TeamBasicInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("logo")]
        public string? Logo { get; set; }

        [JsonPropertyName("winner")]
        public bool? Winner { get; set; }
    }

    public sealed class GoalsInfo
    {
        [JsonPropertyName("home")]
        public int? Home { get; set; }

        [JsonPropertyName("away")]
        public int? Away { get; set; }
    }

    public sealed class ScoreInfo
    {
        [JsonPropertyName("halftime")]
        public GoalsInfo Halftime { get; set; } = new();

        [JsonPropertyName("fulltime")]
        public GoalsInfo Fulltime { get; set; } = new();

        [JsonPropertyName("extratime")]
        public GoalsInfo? Extratime { get; set; }

        [JsonPropertyName("penalty")]
        public GoalsInfo? Penalty { get; set; }
    }

    // Lineups endpoint response
    public sealed class LineupResponse
    {
        [JsonPropertyName("team")]
        public TeamBasicInfo Team { get; set; } = new();

        [JsonPropertyName("formation")]
        public string? Formation { get; set; }

        [JsonPropertyName("startXI")]
        public List<PlayerEntry> StartXI { get; set; } = new();

        [JsonPropertyName("substitutes")]
        public List<PlayerEntry> Substitutes { get; set; } = new();

        [JsonPropertyName("coach")]
        public CoachInfo? Coach { get; set; }
    }

    public sealed class PlayerEntry
    {
        [JsonPropertyName("player")]
        public PlayerInfo Player { get; set; } = new();
    }

    public sealed class PlayerInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("number")]
        public int? Number { get; set; }

        [JsonPropertyName("pos")]
        public string? Pos { get; set; }

        [JsonPropertyName("grid")]
        public string? Grid { get; set; }
    }

    public sealed class CoachInfo
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("photo")]
        public string? Photo { get; set; }
    }

    // Statistics endpoint response
    public sealed class StatisticsResponse
    {
        [JsonPropertyName("team")]
        public TeamBasicInfo Team { get; set; } = new();

        [JsonPropertyName("statistics")]
        public List<StatisticItem> Statistics { get; set; } = new();
    }

    public sealed class StatisticItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public object? Value { get; set; }
    }

    // Events endpoint response
    public sealed class EventResponse
    {
        [JsonPropertyName("time")]
        public TimeInfo Time { get; set; } = new();

        [JsonPropertyName("team")]
        public TeamBasicInfo Team { get; set; } = new();

        [JsonPropertyName("player")]
        public PlayerInfo Player { get; set; } = new();

        [JsonPropertyName("assist")]
        public PlayerInfo? Assist { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("detail")]
        public string? Detail { get; set; }

        [JsonPropertyName("comments")]
        public string? Comments { get; set; }
    }

    public sealed class TimeInfo
    {
        [JsonPropertyName("elapsed")]
        public int Elapsed { get; set; }

        [JsonPropertyName("extra")]
        public int? Extra { get; set; }
    }

    // Leagues validation endpoint
    public sealed class LeagueValidationResponse
    {
        [JsonPropertyName("league")]
        public LeagueDetailInfo League { get; set; } = new();

        [JsonPropertyName("country")]
        public CountryInfo Country { get; set; } = new();

        [JsonPropertyName("seasons")]
        public List<SeasonInfo> Seasons { get; set; } = new();
    }

    public sealed class LeagueDetailInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("logo")]
        public string? Logo { get; set; }
    }

    public sealed class CountryInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("flag")]
        public string? Flag { get; set; }
    }

    public sealed class SeasonInfo
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("start")]
        public string Start { get; set; } = string.Empty;

        [JsonPropertyName("end")]
        public string End { get; set; } = string.Empty;

        [JsonPropertyName("current")]
        public bool Current { get; set; }

        [JsonPropertyName("coverage")]
        public CoverageInfo Coverage { get; set; } = new();
    }

    public sealed class CoverageInfo
    {
        [JsonPropertyName("fixtures")]
        public FixturesCoverage Fixtures { get; set; } = new();

        [JsonPropertyName("standings")]
        public bool Standings { get; set; }

        [JsonPropertyName("players")]
        public bool Players { get; set; }

        [JsonPropertyName("top_scorers")]
        public bool TopScorers { get; set; }

        [JsonPropertyName("top_assists")]
        public bool TopAssists { get; set; }

        [JsonPropertyName("top_cards")]
        public bool TopCards { get; set; }

        [JsonPropertyName("injuries")]
        public bool Injuries { get; set; }

        [JsonPropertyName("predictions")]
        public bool Predictions { get; set; }

        [JsonPropertyName("odds")]
        public bool Odds { get; set; }
    }

    public sealed class FixturesCoverage
    {
        [JsonPropertyName("events")]
        public bool Events { get; set; }

        [JsonPropertyName("lineups")]
        public bool Lineups { get; set; }

        [JsonPropertyName("statistics_fixtures")]
        public bool StatisticsFixtures { get; set; }

        [JsonPropertyName("statistics_players")]
        public bool StatisticsPlayers { get; set; }
    }

    // Standings endpoint response
    public sealed class StandingsResponse
    {
        [JsonPropertyName("league")]
        public StandingsLeagueInfo League { get; set; } = new();
    }

    public sealed class StandingsLeagueInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("logo")]
        public string? Logo { get; set; }

        [JsonPropertyName("flag")]
        public string? Flag { get; set; }

        [JsonPropertyName("season")]
        public int Season { get; set; }

        [JsonPropertyName("standings")]
        public List<List<StandingEntry>> Standings { get; set; } = new();
    }

    public sealed class StandingEntry
    {
        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("team")]
        public TeamBasicInfo Team { get; set; } = new();

        [JsonPropertyName("points")]
        public int Points { get; set; }

        [JsonPropertyName("goalsDiff")]
        public int GoalsDiff { get; set; }

        [JsonPropertyName("group")]
        public string? Group { get; set; }

        [JsonPropertyName("form")]
        public string? Form { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("all")]
        public StandingStats All { get; set; } = new();

        [JsonPropertyName("home")]
        public StandingStats? Home { get; set; }

        [JsonPropertyName("away")]
        public StandingStats? Away { get; set; }

        [JsonPropertyName("update")]
        public string? Update { get; set; }
    }

    public sealed class StandingStats
    {
        [JsonPropertyName("played")]
        public int Played { get; set; }

        [JsonPropertyName("win")]
        public int Win { get; set; }

        [JsonPropertyName("draw")]
        public int Draw { get; set; }

        [JsonPropertyName("lose")]
        public int Lose { get; set; }

        [JsonPropertyName("goals")]
        public StandingGoals Goals { get; set; } = new();
    }

    public sealed class StandingGoals
    {
        [JsonPropertyName("for")]
        public int For { get; set; }

        [JsonPropertyName("against")]
        public int Against { get; set; }
    }

    // Squad/Players endpoint response
    public sealed class SquadResponse
    {
        [JsonPropertyName("team")]
        public TeamBasicInfo Team { get; set; } = new();

        [JsonPropertyName("players")]
        public List<SquadPlayerInfo> Players { get; set; } = new();
    }

    public sealed class SquadPlayerInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("age")]
        public int? Age { get; set; }

        [JsonPropertyName("number")]
        public int? Number { get; set; }

        [JsonPropertyName("position")]
        public string? Position { get; set; }

        [JsonPropertyName("photo")]
        public string? Photo { get; set; }
    }

    // Type aliases for convenience
    public class TeamsResponse : ApiFootballResponse<TeamResponse> { }
    public class FixturesResponse : ApiFootballResponse<FixtureResponse> { }
    public class LineupsResponse : ApiFootballResponse<LineupResponse> { }
    public class StatisticsResponseWrapper : ApiFootballResponse<StatisticsResponse> { }
    public class EventsResponse : ApiFootballResponse<EventResponse> { }
    public class LeaguesResponse : ApiFootballResponse<LeagueValidationResponse> { }
    public class StandingsResponseWrapper : ApiFootballResponse<StandingsResponse> { }
    public class SquadsResponse : ApiFootballResponse<SquadResponse> { }
}
