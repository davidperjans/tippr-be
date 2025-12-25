using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Admin.ApiFootball.Commands.SyncTournamentBaseline
{
    public class SyncTournamentBaselineCommandHandler
        : IRequestHandler<SyncTournamentBaselineCommand, Result<SyncTournamentBaselineResult>>
    {
        private readonly ITipprDbContext _db;
        private readonly IApiFootballClient _apiClient;
        private readonly ILogger<SyncTournamentBaselineCommandHandler> _logger;

        private const string Provider = "ApiFootball";
        private const string ResourceTeams = "Teams";
        private const string ResourceFixtures = "Fixtures";
        private static readonly TimeSpan TeamsTtl = TimeSpan.FromHours(24);
        private static readonly TimeSpan FixturesTtl = TimeSpan.FromHours(6);

        public SyncTournamentBaselineCommandHandler(
            ITipprDbContext db,
            IApiFootballClient apiClient,
            ILogger<SyncTournamentBaselineCommandHandler> logger)
        {
            _db = db;
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<Result<SyncTournamentBaselineResult>> Handle(
            SyncTournamentBaselineCommand request,
            CancellationToken ct)
        {
            var tournament = await _db.Tournaments
                .FirstOrDefaultAsync(t => t.Id == request.TournamentId, ct);

            if (tournament == null)
                return Result<SyncTournamentBaselineResult>.NotFound("Tournament not found", "admin.tournament_not_found");

            if (!tournament.ApiFootballEnabled)
                return Result<SyncTournamentBaselineResult>.BusinessRule(
                    "API-FOOTBALL is not enabled for this tournament", "admin.apifootball_not_enabled");

            if (!tournament.ApiFootballLeagueId.HasValue || !tournament.ApiFootballSeason.HasValue)
                return Result<SyncTournamentBaselineResult>.BusinessRule(
                    "Tournament does not have API-FOOTBALL league ID or season configured",
                    "admin.apifootball_not_configured");

            var leagueId = tournament.ApiFootballLeagueId.Value;
            var season = tournament.ApiFootballSeason.Value;

            // Check TTL for teams sync
            var teamsSyncState = await GetOrCreateSyncState(tournament.Id, ResourceTeams, ct);
            if (!request.Force && teamsSyncState.NextAllowedSyncAt > DateTime.UtcNow)
            {
                _logger.LogInformation(
                    "Skipping teams sync - TTL not expired. Next allowed: {NextAllowed}",
                    teamsSyncState.NextAllowedSyncAt);
            }

            // Check TTL for fixtures sync
            var fixturesSyncState = await GetOrCreateSyncState(tournament.Id, ResourceFixtures, ct);
            if (!request.Force && fixturesSyncState.NextAllowedSyncAt > DateTime.UtcNow)
            {
                _logger.LogInformation(
                    "Skipping fixtures sync - TTL not expired. Next allowed: {NextAllowed}",
                    fixturesSyncState.NextAllowedSyncAt);
            }

            var result = new SyncTournamentBaselineResult
            {
                SyncedAt = DateTime.UtcNow
            };

            var warnings = new List<string>();
            var unmappedTeams = new List<string>();

            // === STEP 1: Sync Teams & Venues ===
            if (request.Force || teamsSyncState.NextAllowedSyncAt <= DateTime.UtcNow)
            {
                var teamsResult = await _apiClient.GetTeamsAsync(leagueId, season, ct);
                if (!teamsResult.Success)
                {
                    return Result<SyncTournamentBaselineResult>.Failure(
                        $"Failed to fetch teams from API-FOOTBALL: {teamsResult.ErrorMessage}");
                }

                var (teamsUpdated, teamsCreated, venuesUpserted, teamsUnmapped, teamWarnings) =
                    await SyncTeamsAndVenues(tournament.Id, teamsResult.Teams, request.CreateMissingTeams, ct);

                result = result with
                {
                    TeamsUpdated = teamsUpdated,
                    TeamsCreated = teamsCreated,
                    TeamsUnmapped = teamsUnmapped,
                    VenuesUpserted = venuesUpserted
                };
                unmappedTeams.AddRange(teamWarnings.Where(w => w.StartsWith("Unmapped:")));
                warnings.AddRange(teamWarnings.Where(w => !w.StartsWith("Unmapped:")));

                // Update sync state
                teamsSyncState.LastSyncedAt = DateTime.UtcNow;
                teamsSyncState.NextAllowedSyncAt = DateTime.UtcNow.Add(TeamsTtl);
                teamsSyncState.LastError = null;
            }

            // === STEP 2: Sync Fixtures/Matches ===
            if (request.Force || fixturesSyncState.NextAllowedSyncAt <= DateTime.UtcNow)
            {
                var fixturesResult = await _apiClient.GetFixturesAsync(leagueId, season, ct);
                if (!fixturesResult.Success)
                {
                    return Result<SyncTournamentBaselineResult>.Failure(
                        $"Failed to fetch fixtures from API-FOOTBALL: {fixturesResult.ErrorMessage}");
                }

                var (matchesUpserted, matchesLinked, matchesSkipped, teamsCreatedFromFixtures, fixtureWarnings) =
                    await SyncFixtures(tournament.Id, fixturesResult.Fixtures, request.CreateMissingTeams, ct);

                result = result with
                {
                    TeamsCreated = result.TeamsCreated + teamsCreatedFromFixtures,
                    MatchesUpserted = matchesUpserted,
                    MatchesLinked = matchesLinked,
                    MatchesSkipped = matchesSkipped
                };
                warnings.AddRange(fixtureWarnings);

                // Update sync state
                fixturesSyncState.LastSyncedAt = DateTime.UtcNow;
                fixturesSyncState.NextAllowedSyncAt = DateTime.UtcNow.Add(FixturesTtl);
                fixturesSyncState.LastError = null;
            }

            await _db.SaveChangesAsync(ct);

            result = result with
            {
                UnmappedTeams = unmappedTeams.Select(w => w.Replace("Unmapped:", "").Trim()).ToList(),
                Warnings = warnings
            };

            _logger.LogInformation(
                "Baseline sync completed for tournament {TournamentId}. " +
                "Teams: {TeamsUpdated} updated, {TeamsCreated} created, {TeamsUnmapped} unmapped. " +
                "Venues: {VenuesUpserted}. " +
                "Matches: {MatchesUpserted} upserted, {MatchesLinked} linked, {MatchesSkipped} skipped.",
                tournament.Id, result.TeamsUpdated, result.TeamsCreated, result.TeamsUnmapped,
                result.VenuesUpserted, result.MatchesUpserted, result.MatchesLinked, result.MatchesSkipped);

            return Result<SyncTournamentBaselineResult>.Success(result);
        }

        private async Task<(int teamsUpdated, int teamsCreated, int venuesUpserted, int teamsUnmapped, List<string> warnings)>
            SyncTeamsAndVenues(Guid tournamentId, List<ApiFootballTeam> apiTeams, bool createMissingTeams, CancellationToken ct)
        {
            var warnings = new List<string>();
            var teamsUpdated = 0;
            var teamsCreated = 0;
            var teamsUnmapped = 0;
            var venuesUpserted = 0;

            // Load existing teams for this tournament
            var existingTeams = await _db.Teams
                .Where(t => t.TournamentId == tournamentId)
                .ToListAsync(ct);

            // Load all venues for matching
            var existingVenues = await _db.Venues.ToListAsync(ct);
            var venuesByApiId = existingVenues
                .Where(v => v.ApiFootballId.HasValue)
                .ToDictionary(v => v.ApiFootballId!.Value);

            foreach (var apiTeam in apiTeams)
            {
                // === Upsert Venue ===
                Guid? venueId = null;
                if (apiTeam.Venue != null && !string.IsNullOrWhiteSpace(apiTeam.Venue.Name))
                {
                    var venue = await UpsertVenue(apiTeam.Venue, venuesByApiId, ct);
                    venueId = venue.Id;
                    if (!existingVenues.Any(v => v.Id == venue.Id))
                    {
                        venuesUpserted++;
                        existingVenues.Add(venue);
                        if (venue.ApiFootballId.HasValue)
                            venuesByApiId[venue.ApiFootballId.Value] = venue;
                    }
                }

                // === Match Team ===
                var matchedTeam = MatchTeam(existingTeams, apiTeam);
                if (matchedTeam == null)
                {
                    if (createMissingTeams)
                    {
                        // Create new team from API-FOOTBALL data
                        var newTeam = new Team
                        {
                            Id = Guid.NewGuid(),
                            TournamentId = tournamentId,
                            ApiFootballId = apiTeam.ApiFootballId,
                            Name = apiTeam.Name,
                            Code = apiTeam.Code,
                            LogoUrl = apiTeam.LogoUrl,
                            FoundedYear = apiTeam.FoundedYear,
                            VenueId = venueId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _db.Teams.Add(newTeam);
                        existingTeams.Add(newTeam);
                        teamsCreated++;

                        _logger.LogInformation("Created team {TeamName} (ApiFootballId: {ApiFootballId})",
                            apiTeam.Name, apiTeam.ApiFootballId);
                    }
                    else
                    {
                        teamsUnmapped++;
                        warnings.Add($"Unmapped: {apiTeam.Name} (ApiFootballId: {apiTeam.ApiFootballId})");
                    }
                    continue;
                }

                // Update team with API-FOOTBALL data
                matchedTeam.ApiFootballId = apiTeam.ApiFootballId;

                // Preserve localized name: if current Name differs from API name and DisplayName not set,
                // move current Name to DisplayName (e.g., Swedish name) and set Name to English
                if (!string.IsNullOrWhiteSpace(apiTeam.Name) &&
                    !matchedTeam.Name.Equals(apiTeam.Name, StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrWhiteSpace(matchedTeam.DisplayName))
                {
                    matchedTeam.DisplayName = matchedTeam.Name;  // Preserve Swedish/localized name
                    matchedTeam.Name = apiTeam.Name;  // Set English/canonical name

                    _logger.LogInformation(
                        "Preserved localized name for team: '{DisplayName}' -> Name: '{Name}' (ApiFootballId: {ApiFootballId})",
                        matchedTeam.DisplayName, matchedTeam.Name, apiTeam.ApiFootballId);
                }

                if (!string.IsNullOrWhiteSpace(apiTeam.LogoUrl))
                    matchedTeam.LogoUrl = apiTeam.LogoUrl;
                if (apiTeam.FoundedYear.HasValue)
                    matchedTeam.FoundedYear = apiTeam.FoundedYear;
                if (venueId.HasValue)
                    matchedTeam.VenueId = venueId;

                matchedTeam.UpdatedAt = DateTime.UtcNow;
                teamsUpdated++;
            }

            return (teamsUpdated, teamsCreated, venuesUpserted, teamsUnmapped, warnings);
        }

        private Team? MatchTeam(List<Team> existingTeams, ApiFootballTeam apiTeam)
        {
            // Priority 1: Match by ApiFootballId (if already set)
            var byApiId = existingTeams.FirstOrDefault(t =>
                t.ApiFootballId.HasValue && t.ApiFootballId.Value == apiTeam.ApiFootballId);
            if (byApiId != null) return byApiId;

            // Priority 2: Match by Code (most reliable for national teams)
            if (!string.IsNullOrWhiteSpace(apiTeam.Code))
            {
                var byCode = existingTeams.FirstOrDefault(t =>
                    !string.IsNullOrWhiteSpace(t.Code) &&
                    t.Code.Equals(apiTeam.Code, StringComparison.OrdinalIgnoreCase));
                if (byCode != null) return byCode;
            }

            // Priority 3: Match by exact normalized name only
            // Fuzzy matching removed - caused false matches like "South Korea" <-> "South Africa"
            var normalizedApiName = NormalizeName(apiTeam.Name);
            var byName = existingTeams.FirstOrDefault(t =>
                NormalizeName(t.Name).Equals(normalizedApiName, StringComparison.OrdinalIgnoreCase));

            return byName;
        }

        private async Task<Venue> UpsertVenue(ApiFootballVenue apiVenue, Dictionary<int, Venue> venuesByApiId, CancellationToken ct)
        {
            // Try to find existing venue by ApiFootballId
            if (apiVenue.ApiFootballId.HasValue && venuesByApiId.TryGetValue(apiVenue.ApiFootballId.Value, out var existingById))
            {
                // Update existing venue
                existingById.Name = apiVenue.Name ?? existingById.Name;
                existingById.Address = apiVenue.Address ?? existingById.Address;
                existingById.City = apiVenue.City ?? existingById.City;
                existingById.Capacity = apiVenue.Capacity ?? existingById.Capacity;
                existingById.Surface = apiVenue.Surface ?? existingById.Surface;
                existingById.ImageUrl = apiVenue.ImageUrl ?? existingById.ImageUrl;
                existingById.UpdatedAt = DateTime.UtcNow;
                return existingById;
            }

            // Try to find by normalized Name + City
            var normalizedName = NormalizeName(apiVenue.Name ?? "");
            var normalizedCity = NormalizeName(apiVenue.City ?? "");

            var existingByNameCity = await _db.Venues
                .FirstOrDefaultAsync(v =>
                    v.Name != null && v.City != null &&
                    v.Name.ToLower() == normalizedName.ToLower() &&
                    v.City.ToLower() == normalizedCity.ToLower(), ct);

            if (existingByNameCity != null)
            {
                existingByNameCity.ApiFootballId = apiVenue.ApiFootballId;
                existingByNameCity.Address = apiVenue.Address ?? existingByNameCity.Address;
                existingByNameCity.Capacity = apiVenue.Capacity ?? existingByNameCity.Capacity;
                existingByNameCity.Surface = apiVenue.Surface ?? existingByNameCity.Surface;
                existingByNameCity.ImageUrl = apiVenue.ImageUrl ?? existingByNameCity.ImageUrl;
                existingByNameCity.UpdatedAt = DateTime.UtcNow;
                return existingByNameCity;
            }

            // Create new venue
            var newVenue = new Venue
            {
                Id = Guid.NewGuid(),
                ApiFootballId = apiVenue.ApiFootballId,
                Name = apiVenue.Name ?? "Unknown",
                Address = apiVenue.Address,
                City = apiVenue.City,
                Capacity = apiVenue.Capacity,
                Surface = apiVenue.Surface,
                ImageUrl = apiVenue.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Venues.Add(newVenue);
            return newVenue;
        }

        private async Task<(int upserted, int linked, int skipped, int teamsCreated, List<string> warnings)>
            SyncFixtures(Guid tournamentId, List<ApiFootballFixture> apiFixtures, bool createMissingTeams, CancellationToken ct)
        {
            var warnings = new List<string>();
            var upserted = 0;
            var linked = 0;
            var skipped = 0;
            var teamsCreated = 0;

            // Load existing matches and teams
            var existingMatches = await _db.Matches
                .Where(m => m.TournamentId == tournamentId)
                .ToListAsync(ct);

            var teams = await _db.Teams
                .Where(t => t.TournamentId == tournamentId && t.ApiFootballId.HasValue)
                .ToDictionaryAsync(t => t.ApiFootballId!.Value, ct);

            var venues = await _db.Venues
                .Where(v => v.ApiFootballId.HasValue)
                .ToDictionaryAsync(v => v.ApiFootballId!.Value, ct);

            // Track which missing team IDs we've already tried to fetch
            var fetchedTeamIds = new HashSet<int>();

            foreach (var apiFixture in apiFixtures)
            {
                // Find home and away teams by their ApiFootballId
                teams.TryGetValue(apiFixture.HomeTeamApiId, out var homeTeam);
                teams.TryGetValue(apiFixture.AwayTeamApiId, out var awayTeam);

                // Try to fetch missing teams if createMissingTeams is enabled
                if (createMissingTeams)
                {
                    if (homeTeam == null && !fetchedTeamIds.Contains(apiFixture.HomeTeamApiId))
                    {
                        fetchedTeamIds.Add(apiFixture.HomeTeamApiId);
                        homeTeam = await FetchAndCreateTeam(tournamentId, apiFixture.HomeTeamApiId, teams, ct);
                        if (homeTeam != null) teamsCreated++;
                    }

                    if (awayTeam == null && !fetchedTeamIds.Contains(apiFixture.AwayTeamApiId))
                    {
                        fetchedTeamIds.Add(apiFixture.AwayTeamApiId);
                        awayTeam = await FetchAndCreateTeam(tournamentId, apiFixture.AwayTeamApiId, teams, ct);
                        if (awayTeam != null) teamsCreated++;
                    }
                }

                if (homeTeam == null || awayTeam == null)
                {
                    skipped++;
                    warnings.Add($"Skipped fixture {apiFixture.ApiFootballId}: Teams not found " +
                        $"(Home: {apiFixture.HomeTeamApiId}, Away: {apiFixture.AwayTeamApiId})");
                    continue;
                }

                // Try to match existing match
                var existingMatch = MatchFixture(existingMatches, apiFixture, homeTeam.Id, awayTeam.Id);

                if (existingMatch != null)
                {
                    // Link/update existing match
                    var wasLinked = !existingMatch.ApiFootballId.HasValue;
                    UpdateMatchFromFixture(existingMatch, apiFixture, homeTeam, awayTeam, venues);

                    if (wasLinked) linked++;
                    else upserted++;
                }
                else
                {
                    // Create new match (this is unusual for baseline sync, but handle it)
                    var newMatch = CreateMatchFromFixture(tournamentId, apiFixture, homeTeam, awayTeam, venues);
                    _db.Matches.Add(newMatch);
                    existingMatches.Add(newMatch);
                    upserted++;
                }
            }

            return (upserted, linked, skipped, teamsCreated, warnings);
        }

        private async Task<Team?> FetchAndCreateTeam(Guid tournamentId, int apiFootballId, Dictionary<int, Team> teams, CancellationToken ct)
        {
            var teamResult = await _apiClient.GetTeamByIdAsync(apiFootballId, ct);
            if (!teamResult.Success || teamResult.Team == null)
            {
                _logger.LogWarning("Failed to fetch team {ApiFootballId} from API-FOOTBALL: {Error}",
                    apiFootballId, teamResult.ErrorMessage);
                return null;
            }

            var apiTeam = teamResult.Team;
            var newTeam = new Team
            {
                Id = Guid.NewGuid(),
                TournamentId = tournamentId,
                ApiFootballId = apiTeam.ApiFootballId,
                Name = apiTeam.Name,
                Code = apiTeam.Code,
                LogoUrl = apiTeam.LogoUrl,
                FoundedYear = apiTeam.FoundedYear,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Teams.Add(newTeam);
            teams[apiFootballId] = newTeam;

            _logger.LogInformation("Created team {TeamName} (ApiFootballId: {ApiFootballId}) from fixture reference",
                apiTeam.Name, apiFootballId);

            return newTeam;
        }

        private Match? MatchFixture(List<Match> existingMatches, ApiFootballFixture apiFixture, Guid homeTeamId, Guid awayTeamId)
        {
            // Priority 1: Match by ApiFootballId
            var byApiId = existingMatches.FirstOrDefault(m =>
                m.ApiFootballId.HasValue && m.ApiFootballId.Value == apiFixture.ApiFootballId);
            if (byApiId != null) return byApiId;

            // Priority 2: Match by teams + datetime (within 2 hour tolerance)
            var tolerance = TimeSpan.FromHours(2);
            var byTeamsAndDate = existingMatches.FirstOrDefault(m =>
                m.HomeTeamId == homeTeamId &&
                m.AwayTeamId == awayTeamId &&
                Math.Abs((m.MatchDate - apiFixture.MatchDateUtc).TotalHours) <= tolerance.TotalHours);

            return byTeamsAndDate;
        }

        private void UpdateMatchFromFixture(
            Match match,
            ApiFootballFixture apiFixture,
            Team homeTeam,
            Team awayTeam,
            Dictionary<int, Venue> venues)
        {
            var wasChanged = false;

            if (!match.ApiFootballId.HasValue || match.ApiFootballId != apiFixture.ApiFootballId)
            {
                match.ApiFootballId = apiFixture.ApiFootballId;
                wasChanged = true;
            }

            match.MatchDate = apiFixture.MatchDateUtc;
            match.Status = MapStatus(apiFixture.StatusShort);
            match.Stage = MapStage(apiFixture.Round);

            // Update scores if finished
            if (apiFixture.HomeScore.HasValue && apiFixture.AwayScore.HasValue)
            {
                if (match.HomeScore != apiFixture.HomeScore || match.AwayScore != apiFixture.AwayScore)
                {
                    match.HomeScore = apiFixture.HomeScore;
                    match.AwayScore = apiFixture.AwayScore;
                    match.ResultVersion++;
                    wasChanged = true;
                }
            }

            // Update venue
            if (apiFixture.Venue != null)
            {
                match.VenueName = apiFixture.Venue.Name;
                match.VenueCity = apiFixture.Venue.City;
                if (apiFixture.Venue.ApiFootballId.HasValue &&
                    venues.TryGetValue(apiFixture.Venue.ApiFootballId.Value, out var venue))
                {
                    match.VenueId = venue.Id;
                }
            }

            if (wasChanged)
                match.UpdatedAt = DateTime.UtcNow;
        }

        private Match CreateMatchFromFixture(
            Guid tournamentId,
            ApiFootballFixture apiFixture,
            Team homeTeam,
            Team awayTeam,
            Dictionary<int, Venue> venues)
        {
            Guid? venueId = null;
            if (apiFixture.Venue?.ApiFootballId.HasValue == true &&
                venues.TryGetValue(apiFixture.Venue.ApiFootballId.Value, out var venue))
            {
                venueId = venue.Id;
            }

            return new Match
            {
                Id = Guid.NewGuid(),
                TournamentId = tournamentId,
                ApiFootballId = apiFixture.ApiFootballId,
                HomeTeamId = homeTeam.Id,
                AwayTeamId = awayTeam.Id,
                MatchDate = apiFixture.MatchDateUtc,
                Status = MapStatus(apiFixture.StatusShort),
                Stage = MapStage(apiFixture.Round),
                HomeScore = apiFixture.HomeScore,
                AwayScore = apiFixture.AwayScore,
                VenueId = venueId,
                VenueName = apiFixture.Venue?.Name,
                VenueCity = apiFixture.Venue?.City,
                ResultVersion = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private async Task<ExternalSyncState> GetOrCreateSyncState(Guid tournamentId, string resource, CancellationToken ct)
        {
            var state = await _db.ExternalSyncStates
                .FirstOrDefaultAsync(s =>
                    s.TournamentId == tournamentId &&
                    s.Provider == Provider &&
                    s.Resource == resource, ct);

            if (state == null)
            {
                state = new ExternalSyncState
                {
                    Id = Guid.NewGuid(),
                    TournamentId = tournamentId,
                    Provider = Provider,
                    Resource = resource,
                    LastSyncedAt = DateTime.MinValue,
                    NextAllowedSyncAt = DateTime.MinValue,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.ExternalSyncStates.Add(state);
            }

            return state;
        }

        private static MatchStatus MapStatus(string statusShort) => statusShort switch
        {
            "TBD" or "NS" => MatchStatus.Scheduled,
            "1H" or "HT" or "2H" or "ET" or "BT" or "P" or "LIVE" => MatchStatus.Live,
            "FT" or "AET" or "PEN" => MatchStatus.FullTime,
            "PST" or "SUSP" or "INT" => MatchStatus.Postponed,
            "CANC" or "ABD" or "AWD" or "WO" => MatchStatus.Cancelled,
            _ => MatchStatus.Scheduled
        };

        private static MatchStage MapStage(string? round)
        {
            if (string.IsNullOrWhiteSpace(round))
                return MatchStage.Group;

            var lower = round.ToLowerInvariant();

            if (lower.Contains("group"))
                return MatchStage.Group;
            if (lower.Contains("16") || lower.Contains("round of 16"))
                return MatchStage.RoundOf16;
            if (lower.Contains("quarter"))
                return MatchStage.QuarterFinal;
            if (lower.Contains("semi"))
                return MatchStage.SemiFinal;
            if (lower.Contains("final"))
                return MatchStage.Final;

            return MatchStage.Group;
        }

        private static string NormalizeName(string name)
        {
            return name.Trim()
                .Replace("-", " ")
                .Replace(".", "")
                .Replace("'", "")
                .ToLowerInvariant();
        }
    }
}
