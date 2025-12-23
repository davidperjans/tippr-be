using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Admin.ApiFootball.Commands.SyncTournamentResults
{
    public class SyncTournamentResultsCommandHandler
        : IRequestHandler<SyncTournamentResultsCommand, Result<SyncTournamentResultsResult>>
    {
        private readonly ITipprDbContext _db;
        private readonly IApiFootballClient _apiClient;
        private readonly ILogger<SyncTournamentResultsCommandHandler> _logger;

        private const string Provider = "ApiFootball";
        private const string ResourceResults = "Results";
        private const int BatchSize = 20; // API-FOOTBALL limit per request
        private static readonly TimeSpan LiveMatchTtl = TimeSpan.FromSeconds(90);
        private static readonly TimeSpan NonLiveMatchTtl = TimeSpan.FromMinutes(15);

        public SyncTournamentResultsCommandHandler(
            ITipprDbContext db,
            IApiFootballClient apiClient,
            ILogger<SyncTournamentResultsCommandHandler> logger)
        {
            _db = db;
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<Result<SyncTournamentResultsResult>> Handle(
            SyncTournamentResultsCommand request,
            CancellationToken ct)
        {
            var tournament = await _db.Tournaments
                .FirstOrDefaultAsync(t => t.Id == request.TournamentId, ct);

            if (tournament == null)
                return Result<SyncTournamentResultsResult>.NotFound("Tournament not found", "admin.tournament_not_found");

            if (!tournament.ApiFootballEnabled)
                return Result<SyncTournamentResultsResult>.BusinessRule(
                    "API-FOOTBALL is not enabled for this tournament", "admin.apifootball_not_enabled");

            // Get matches that need syncing:
            // - Have ApiFootballId set
            // - Status is not FullTime OR match date is within relevant time window
            var now = DateTime.UtcNow;
            var matchesToSync = await _db.Matches
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .Where(m => m.TournamentId == request.TournamentId)
                .Where(m => m.ApiFootballId.HasValue)
                .Where(m =>
                    m.Status != MatchStatus.FullTime ||
                    (m.MatchDate >= now.AddHours(-6) && m.MatchDate <= now.AddHours(2)))
                .ToListAsync(ct);

            if (matchesToSync.Count == 0)
            {
                return Result<SyncTournamentResultsResult>.Success(new SyncTournamentResultsResult
                {
                    MatchesUpdated = 0,
                    MatchesUnchanged = 0,
                    SyncedAt = now,
                    Warnings = new List<string> { "No matches to sync (all finished or outside time window)" }
                });
            }

            // Check TTL if not forcing
            if (!request.Force)
            {
                var syncState = await GetOrCreateSyncState(tournament.Id, ct);
                if (syncState.NextAllowedSyncAt > now)
                {
                    // Check if we have any live matches that override global TTL
                    var hasLiveMatches = matchesToSync.Any(m => m.Status == MatchStatus.Live);
                    if (!hasLiveMatches)
                    {
                        return Result<SyncTournamentResultsResult>.Success(new SyncTournamentResultsResult
                        {
                            MatchesUpdated = 0,
                            MatchesUnchanged = matchesToSync.Count,
                            SyncedAt = now,
                            Warnings = new List<string>
                            {
                                $"TTL not expired. Next allowed sync: {syncState.NextAllowedSyncAt:u}"
                            }
                        });
                    }
                }
            }

            var updates = new List<MatchUpdateInfo>();
            var warnings = new List<string>();
            var matchesUpdated = 0;
            var matchesUnchanged = 0;
            var matchesNotFound = 0;
            var apiCallsMade = 0;

            // Get fixture IDs to sync
            var fixtureIds = matchesToSync
                .Where(m => m.ApiFootballId.HasValue)
                .Select(m => m.ApiFootballId!.Value)
                .ToList();

            // Batch the API calls
            var batches = fixtureIds
                .Select((id, idx) => new { id, idx })
                .GroupBy(x => x.idx / BatchSize)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();

            var allFixtures = new List<ApiFootballFixture>();

            foreach (var batch in batches)
            {
                var result = await _apiClient.GetFixturesByIdsAsync(batch, ct);
                apiCallsMade++;

                if (!result.Success)
                {
                    warnings.Add($"Failed to fetch batch: {result.ErrorMessage}");
                    continue;
                }

                allFixtures.AddRange(result.Fixtures);
            }

            // Create lookup for quick access
            var fixturesLookup = allFixtures.ToDictionary(f => f.ApiFootballId);

            // Update matches
            foreach (var match in matchesToSync)
            {
                if (!match.ApiFootballId.HasValue)
                    continue;

                if (!fixturesLookup.TryGetValue(match.ApiFootballId.Value, out var apiFixture))
                {
                    matchesNotFound++;
                    warnings.Add($"Fixture {match.ApiFootballId} not found in API response");
                    continue;
                }

                var updateInfo = UpdateMatchFromApiFixture(match, apiFixture);
                if (updateInfo != null)
                {
                    updates.Add(updateInfo);
                    matchesUpdated++;
                }
                else
                {
                    matchesUnchanged++;
                }
            }

            // Update sync state
            var state = await GetOrCreateSyncState(tournament.Id, ct);
            state.LastSyncedAt = now;

            // TTL depends on whether we have live matches
            var hasLive = matchesToSync.Any(m => m.Status == MatchStatus.Live);
            state.NextAllowedSyncAt = hasLive
                ? now.Add(LiveMatchTtl)
                : now.Add(NonLiveMatchTtl);
            state.UpdatedAt = now;

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Results sync completed for tournament {TournamentId}. " +
                "Updated: {Updated}, Unchanged: {Unchanged}, NotFound: {NotFound}, API calls: {ApiCalls}",
                tournament.Id, matchesUpdated, matchesUnchanged, matchesNotFound, apiCallsMade);

            return Result<SyncTournamentResultsResult>.Success(new SyncTournamentResultsResult
            {
                MatchesUpdated = matchesUpdated,
                MatchesUnchanged = matchesUnchanged,
                MatchesNotFound = matchesNotFound,
                ApiCallsMade = apiCallsMade,
                Updates = updates,
                Warnings = warnings,
                SyncedAt = now
            });
        }

        private MatchUpdateInfo? UpdateMatchFromApiFixture(Match match, ApiFootballFixture apiFixture)
        {
            var oldStatus = match.Status;
            var oldHomeScore = match.HomeScore;
            var oldAwayScore = match.AwayScore;

            var newStatus = MapStatus(apiFixture.StatusShort);
            var newHomeScore = apiFixture.HomeScore;
            var newAwayScore = apiFixture.AwayScore;

            // Check if anything changed
            var statusChanged = oldStatus != newStatus;
            var scoreChanged = oldHomeScore != newHomeScore || oldAwayScore != newAwayScore;

            if (!statusChanged && !scoreChanged)
                return null;

            // Apply updates
            match.Status = newStatus;

            if (newHomeScore.HasValue && newAwayScore.HasValue)
            {
                if (match.HomeScore != newHomeScore || match.AwayScore != newAwayScore)
                {
                    match.HomeScore = newHomeScore;
                    match.AwayScore = newAwayScore;
                    match.ResultVersion++;
                }
            }

            match.UpdatedAt = DateTime.UtcNow;

            return new MatchUpdateInfo
            {
                MatchId = match.Id,
                HomeTeam = match.HomeTeam?.Name ?? "Unknown",
                AwayTeam = match.AwayTeam?.Name ?? "Unknown",
                OldStatus = oldStatus.ToString(),
                NewStatus = newStatus.ToString(),
                OldScore = oldHomeScore.HasValue && oldAwayScore.HasValue
                    ? $"{oldHomeScore}-{oldAwayScore}"
                    : null,
                NewScore = newHomeScore.HasValue && newAwayScore.HasValue
                    ? $"{newHomeScore}-{newAwayScore}"
                    : null
            };
        }

        private async Task<ExternalSyncState> GetOrCreateSyncState(Guid tournamentId, CancellationToken ct)
        {
            var state = await _db.ExternalSyncStates
                .FirstOrDefaultAsync(s =>
                    s.TournamentId == tournamentId &&
                    s.Provider == Provider &&
                    s.Resource == ResourceResults, ct);

            if (state == null)
            {
                state = new ExternalSyncState
                {
                    Id = Guid.NewGuid(),
                    TournamentId = tournamentId,
                    Provider = Provider,
                    Resource = ResourceResults,
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
    }
}
