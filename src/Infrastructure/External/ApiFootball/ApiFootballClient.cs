using Application.Common.Interfaces;
using Infrastructure.External.ApiFootball.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.External.ApiFootball
{
    public sealed class ApiFootballClient : IApiFootballClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<ApiFootballClient> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiFootballClient(HttpClient http, IOptions<ApiFootballOptions> options, ILogger<ApiFootballClient> logger)
        {
            _http = http;
            _logger = logger;
            _http.BaseAddress = new Uri(options.Value.BaseUrl);
            _http.DefaultRequestHeaders.Add("x-apisports-key", options.Value.ApiKey);
        }

        public async Task<ApiFootballTeamsResult> GetTeamsAsync(int leagueId, int season, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Fetching teams from API-FOOTBALL: leagueId={LeagueId}, season={Season}", leagueId, season);

                var response = await _http.GetFromJsonAsync<TeamsResponse>(
                    $"/teams?league={leagueId}&season={season}", JsonOptions, ct);

                if (response == null)
                {
                    return new ApiFootballTeamsResult
                    {
                        Success = false,
                        ErrorMessage = "Empty response from API-FOOTBALL"
                    };
                }

                var teams = response.Response.Select(r => new ApiFootballTeam
                {
                    ApiFootballId = r.Team.Id,
                    Name = r.Team.Name,
                    Code = r.Team.Code,
                    FoundedYear = r.Team.Founded,
                    LogoUrl = r.Team.Logo,
                    IsNational = r.Team.National,
                    Venue = r.Venue != null ? MapVenue(r.Venue) : null
                }).ToList();

                _logger.LogInformation("Fetched {Count} teams from API-FOOTBALL", teams.Count);

                return new ApiFootballTeamsResult
                {
                    Success = true,
                    Teams = teams
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching teams from API-FOOTBALL");
                return new ApiFootballTeamsResult
                {
                    Success = false,
                    ErrorMessage = $"HTTP error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teams from API-FOOTBALL");
                return new ApiFootballTeamsResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiFootballFixturesResult> GetFixturesAsync(int leagueId, int season, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Fetching fixtures from API-FOOTBALL: leagueId={LeagueId}, season={Season}", leagueId, season);

                var response = await _http.GetFromJsonAsync<FixturesResponse>(
                    $"/fixtures?league={leagueId}&season={season}", JsonOptions, ct);

                if (response == null)
                {
                    return new ApiFootballFixturesResult
                    {
                        Success = false,
                        ErrorMessage = "Empty response from API-FOOTBALL"
                    };
                }

                var fixtures = MapFixtures(response.Response);

                _logger.LogInformation("Fetched {Count} fixtures from API-FOOTBALL", fixtures.Count);

                return new ApiFootballFixturesResult
                {
                    Success = true,
                    Fixtures = fixtures
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching fixtures from API-FOOTBALL");
                return new ApiFootballFixturesResult
                {
                    Success = false,
                    ErrorMessage = $"HTTP error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching fixtures from API-FOOTBALL");
                return new ApiFootballFixturesResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiFootballFixturesResult> GetFixturesByIdsAsync(IEnumerable<int> fixtureIds, CancellationToken ct = default)
        {
            try
            {
                var idsList = fixtureIds.ToList();
                if (idsList.Count == 0)
                {
                    return new ApiFootballFixturesResult { Success = true, Fixtures = new() };
                }

                // API-FOOTBALL v3 uses dash-separated IDs
                var ids = string.Join("-", idsList);
                _logger.LogInformation("Fetching fixtures by IDs from API-FOOTBALL: ids={Ids}", ids);

                var response = await _http.GetFromJsonAsync<FixturesResponse>(
                    $"/fixtures?ids={ids}", JsonOptions, ct);

                if (response == null)
                {
                    return new ApiFootballFixturesResult
                    {
                        Success = false,
                        ErrorMessage = "Empty response from API-FOOTBALL"
                    };
                }

                var fixtures = MapFixtures(response.Response);

                _logger.LogInformation("Fetched {Count} fixtures by IDs from API-FOOTBALL", fixtures.Count);

                return new ApiFootballFixturesResult
                {
                    Success = true,
                    Fixtures = fixtures
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching fixtures by IDs from API-FOOTBALL");
                return new ApiFootballFixturesResult
                {
                    Success = false,
                    ErrorMessage = $"HTTP error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching fixtures by IDs from API-FOOTBALL");
                return new ApiFootballFixturesResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiFootballLineupsResult> GetLineupsAsync(int fixtureId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Fetching lineups from API-FOOTBALL: fixtureId={FixtureId}", fixtureId);

                var responseMessage = await _http.GetAsync($"/fixtures/lineups?fixture={fixtureId}", ct);
                var rawJson = await responseMessage.Content.ReadAsStringAsync(ct);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    return new ApiFootballLineupsResult
                    {
                        Success = false,
                        ErrorMessage = $"API returned {responseMessage.StatusCode}"
                    };
                }

                var response = JsonSerializer.Deserialize<LineupsResponse>(rawJson, JsonOptions);

                if (response == null)
                {
                    return new ApiFootballLineupsResult
                    {
                        Success = false,
                        ErrorMessage = "Empty response from API-FOOTBALL"
                    };
                }

                var lineups = response.Response.Select(r => new ApiFootballLineup
                {
                    TeamApiId = r.Team.Id,
                    TeamName = r.Team.Name,
                    Formation = r.Formation,
                    CoachName = r.Coach?.Name,
                    StartingXI = r.StartXI.Select(p => new ApiFootballPlayer
                    {
                        Id = p.Player.Id,
                        Name = p.Player.Name,
                        Number = p.Player.Number,
                        Position = p.Player.Pos
                    }).ToList(),
                    Substitutes = r.Substitutes.Select(p => new ApiFootballPlayer
                    {
                        Id = p.Player.Id,
                        Name = p.Player.Name,
                        Number = p.Player.Number,
                        Position = p.Player.Pos
                    }).ToList()
                }).ToList();

                _logger.LogInformation("Fetched lineups for {Count} teams from API-FOOTBALL", lineups.Count);

                return new ApiFootballLineupsResult
                {
                    Success = true,
                    RawJson = rawJson,
                    Lineups = lineups
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching lineups from API-FOOTBALL");
                return new ApiFootballLineupsResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiFootballLeagueValidationResult> ValidateLeagueAsync(int leagueId, int season, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Validating league from API-FOOTBALL: leagueId={LeagueId}, season={Season}", leagueId, season);

                var response = await _http.GetFromJsonAsync<LeaguesResponse>(
                    $"/leagues?id={leagueId}&season={season}", JsonOptions, ct);

                if (response == null || response.Response.Count == 0)
                {
                    return new ApiFootballLeagueValidationResult
                    {
                        Success = true,
                        IsValid = false,
                        ErrorMessage = "League not found or season not available"
                    };
                }

                var league = response.Response[0];
                var seasonInfo = league.Seasons.FirstOrDefault(s => s.Year == season);

                return new ApiFootballLeagueValidationResult
                {
                    Success = true,
                    IsValid = true,
                    LeagueName = league.League.Name,
                    LeagueType = league.League.Type,
                    Country = league.Country.Name,
                    HasLineupsSupport = seasonInfo?.Coverage.Fixtures.Lineups ?? false,
                    HasEventsSupport = seasonInfo?.Coverage.Fixtures.Events ?? false,
                    HasStatisticsSupport = seasonInfo?.Coverage.Fixtures.StatisticsFixtures ?? false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating league from API-FOOTBALL");
                return new ApiFootballLeagueValidationResult
                {
                    Success = false,
                    IsValid = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiFootballStandingsResult> GetStandingsAsync(int leagueId, int season, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Fetching standings from API-FOOTBALL: leagueId={LeagueId}, season={Season}", leagueId, season);

                var response = await _http.GetFromJsonAsync<StandingsResponseWrapper>(
                    $"/standings?league={leagueId}&season={season}", JsonOptions, ct);

                if (response == null || response.Response.Count == 0)
                {
                    return new ApiFootballStandingsResult
                    {
                        Success = false,
                        ErrorMessage = "Empty response from API-FOOTBALL"
                    };
                }

                var standingsData = response.Response[0];
                var groupStandings = new List<ApiFootballGroupStanding>();

                // API returns standings as a list of lists (each inner list is a group)
                foreach (var group in standingsData.League.Standings)
                {
                    if (group.Count == 0) continue;

                    var groupName = group[0].Group ?? "Unknown";

                    var teamStandings = group.Select(entry => new ApiFootballTeamStanding
                    {
                        TeamApiId = entry.Team.Id,
                        TeamName = entry.Team.Name,
                        Position = entry.Rank,
                        Played = entry.All.Played,
                        Won = entry.All.Win,
                        Drawn = entry.All.Draw,
                        Lost = entry.All.Lose,
                        GoalsFor = entry.All.Goals.For,
                        GoalsAgainst = entry.All.Goals.Against,
                        GoalDifference = entry.GoalsDiff,
                        Points = entry.Points,
                        Form = entry.Form
                    }).ToList();

                    groupStandings.Add(new ApiFootballGroupStanding
                    {
                        GroupName = groupName,
                        Teams = teamStandings
                    });
                }

                _logger.LogInformation("Fetched standings for {Count} groups from API-FOOTBALL", groupStandings.Count);

                return new ApiFootballStandingsResult
                {
                    Success = true,
                    GroupStandings = groupStandings
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching standings from API-FOOTBALL");
                return new ApiFootballStandingsResult
                {
                    Success = false,
                    ErrorMessage = $"HTTP error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching standings from API-FOOTBALL");
                return new ApiFootballStandingsResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiFootballSquadResult> GetSquadAsync(int teamId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Fetching squad from API-FOOTBALL: teamId={TeamId}", teamId);

                var response = await _http.GetFromJsonAsync<SquadsResponse>(
                    $"/players/squads?team={teamId}", JsonOptions, ct);

                if (response == null || response.Response.Count == 0)
                {
                    return new ApiFootballSquadResult
                    {
                        Success = false,
                        ErrorMessage = "Empty response from API-FOOTBALL"
                    };
                }

                var squadData = response.Response[0];
                var players = squadData.Players.Select(p => new ApiFootballSquadPlayer
                {
                    ApiFootballId = p.Id,
                    Name = p.Name,
                    Age = p.Age,
                    Number = p.Number,
                    Position = p.Position,
                    PhotoUrl = p.Photo
                }).ToList();

                _logger.LogInformation("Fetched {Count} players for team {TeamId} from API-FOOTBALL",
                    players.Count, teamId);

                return new ApiFootballSquadResult
                {
                    Success = true,
                    TeamApiId = squadData.Team.Id,
                    TeamName = squadData.Team.Name,
                    Players = players
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error fetching squad from API-FOOTBALL");
                return new ApiFootballSquadResult
                {
                    Success = false,
                    ErrorMessage = $"HTTP error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching squad from API-FOOTBALL");
                return new ApiFootballSquadResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<string> GetLineupsRawJsonAsync(int fixtureId, CancellationToken ct = default)
        {
            var response = await _http.GetAsync($"/fixtures/lineups?fixture={fixtureId}", ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(ct);
        }

        public async Task<string> GetStatisticsRawJsonAsync(int fixtureId, CancellationToken ct = default)
        {
            var response = await _http.GetAsync($"/fixtures/statistics?fixture={fixtureId}", ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(ct);
        }

        public async Task<string> GetEventsRawJsonAsync(int fixtureId, CancellationToken ct = default)
        {
            var response = await _http.GetAsync($"/fixtures/events?fixture={fixtureId}", ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(ct);
        }

        private static ApiFootballVenue MapVenue(VenueInfo v) => new()
        {
            ApiFootballId = v.Id,
            Name = v.Name,
            Address = v.Address,
            City = v.City,
            Capacity = v.Capacity,
            Surface = v.Surface,
            ImageUrl = v.Image
        };

        private static List<ApiFootballFixture> MapFixtures(List<FixtureResponse> responses)
        {
            return responses.Select(r => new ApiFootballFixture
            {
                ApiFootballId = r.Fixture.Id,
                MatchDateUtc = r.Fixture.Date.ToUniversalTime(),
                StatusShort = r.Fixture.Status.Short,
                StatusLong = r.Fixture.Status.Long,
                Elapsed = r.Fixture.Status.Elapsed,
                Round = r.League.Round,
                HomeTeamApiId = r.Teams.Home.Id,
                HomeTeamName = r.Teams.Home.Name,
                AwayTeamApiId = r.Teams.Away.Id,
                AwayTeamName = r.Teams.Away.Name,
                HomeScore = r.Goals.Home,
                AwayScore = r.Goals.Away,
                Venue = r.Fixture.Venue != null ? MapVenue(r.Fixture.Venue) : null
            }).ToList();
        }
    }
}
