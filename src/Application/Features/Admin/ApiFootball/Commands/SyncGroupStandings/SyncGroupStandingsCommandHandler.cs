using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Admin.ApiFootball.Commands.SyncGroupStandings
{
    public class SyncGroupStandingsCommandHandler
        : IRequestHandler<SyncGroupStandingsCommand, Result<SyncGroupStandingsResult>>
    {
        private readonly ITipprDbContext _db;
        private readonly IApiFootballClient _apiClient;
        private readonly ILogger<SyncGroupStandingsCommandHandler> _logger;

        private const string Provider = "ApiFootball";
        private const string ResourceStandings = "Standings";
        private static readonly TimeSpan StandingsTtl = TimeSpan.FromHours(4);

        public SyncGroupStandingsCommandHandler(
            ITipprDbContext db,
            IApiFootballClient apiClient,
            ILogger<SyncGroupStandingsCommandHandler> logger)
        {
            _db = db;
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<Result<SyncGroupStandingsResult>> Handle(
            SyncGroupStandingsCommand request,
            CancellationToken ct)
        {
            var tournament = await _db.Tournaments
                .FirstOrDefaultAsync(t => t.Id == request.TournamentId, ct);

            if (tournament == null)
                return Result<SyncGroupStandingsResult>.NotFound("Tournament not found", "admin.tournament_not_found");

            if (!tournament.ApiFootballEnabled)
                return Result<SyncGroupStandingsResult>.BusinessRule(
                    "API-FOOTBALL is not enabled for this tournament", "admin.apifootball_not_enabled");

            if (!tournament.ApiFootballLeagueId.HasValue || !tournament.ApiFootballSeason.HasValue)
                return Result<SyncGroupStandingsResult>.BusinessRule(
                    "Tournament does not have API-FOOTBALL league ID or season configured",
                    "admin.apifootball_not_configured");

            var leagueId = tournament.ApiFootballLeagueId.Value;
            var season = tournament.ApiFootballSeason.Value;

            // Check TTL for standings sync
            var syncState = await GetOrCreateSyncState(tournament.Id, ct);
            if (!request.Force && syncState.NextAllowedSyncAt > DateTime.UtcNow)
            {
                _logger.LogInformation(
                    "Skipping standings sync - TTL not expired. Next allowed: {NextAllowed}",
                    syncState.NextAllowedSyncAt);

                return Result<SyncGroupStandingsResult>.Success(new SyncGroupStandingsResult
                {
                    SyncedAt = syncState.LastSyncedAt,
                    Warnings = new List<string> { "Sync skipped - TTL not expired" }
                });
            }

            // Fetch standings from API-FOOTBALL
            var standingsResult = await _apiClient.GetStandingsAsync(leagueId, season, ct);
            if (!standingsResult.Success)
            {
                return Result<SyncGroupStandingsResult>.Failure(
                    $"Failed to fetch standings from API-FOOTBALL: {standingsResult.ErrorMessage}");
            }

            var warnings = new List<string>();
            var groupsCreated = 0;
            var groupsUpdated = 0;
            var teamsAssigned = 0;
            var standingsUpserted = 0;

            // Load existing groups for this tournament
            var existingGroups = await _db.Groups
                .Where(g => g.TournamentId == tournament.Id)
                .Include(g => g.Standings)
                .ToListAsync(ct);

            // Load teams for this tournament (with ApiFootballId)
            var teams = await _db.Teams
                .Where(t => t.TournamentId == tournament.Id && t.ApiFootballId.HasValue)
                .ToDictionaryAsync(t => t.ApiFootballId!.Value, ct);

            foreach (var apiGroup in standingsResult.GroupStandings)
            {
                // Parse group name (e.g., "Group A" -> "A")
                var groupName = ParseGroupName(apiGroup.GroupName);

                // Find or create group
                var group = existingGroups.FirstOrDefault(g =>
                    g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));

                if (group == null)
                {
                    group = new Group
                    {
                        Id = Guid.NewGuid(),
                        TournamentId = tournament.Id,
                        Name = groupName,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _db.Groups.Add(group);
                    existingGroups.Add(group);
                    groupsCreated++;

                    _logger.LogInformation("Created group {GroupName} for tournament {TournamentId}",
                        groupName, tournament.Id);
                }
                else
                {
                    group.UpdatedAt = DateTime.UtcNow;
                    groupsUpdated++;
                }

                // Process team standings in this group
                foreach (var apiTeamStanding in apiGroup.Teams)
                {
                    // Find team by ApiFootballId
                    if (!teams.TryGetValue(apiTeamStanding.TeamApiId, out var team))
                    {
                        warnings.Add($"Team not found for ApiFootballId {apiTeamStanding.TeamApiId} ({apiTeamStanding.TeamName})");
                        continue;
                    }

                    // Assign team to group if not already assigned
                    if (team.GroupId != group.Id)
                    {
                        team.GroupId = group.Id;
                        team.UpdatedAt = DateTime.UtcNow;
                        teamsAssigned++;

                        _logger.LogInformation("Assigned team {TeamName} to group {GroupName}",
                            team.Name, groupName);
                    }

                    // Find or create group standing
                    var standing = group.Standings.FirstOrDefault(s => s.TeamId == team.Id);

                    if (standing == null)
                    {
                        standing = new GroupStanding
                        {
                            Id = Guid.NewGuid(),
                            GroupId = group.Id,
                            TeamId = team.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _db.GroupStandings.Add(standing);
                        group.Standings.Add(standing);
                    }

                    // Update standing values
                    standing.Position = apiTeamStanding.Position;
                    standing.Played = apiTeamStanding.Played;
                    standing.Won = apiTeamStanding.Won;
                    standing.Drawn = apiTeamStanding.Drawn;
                    standing.Lost = apiTeamStanding.Lost;
                    standing.GoalsFor = apiTeamStanding.GoalsFor;
                    standing.GoalsAgainst = apiTeamStanding.GoalsAgainst;
                    standing.GoalDifference = apiTeamStanding.GoalDifference;
                    standing.Points = apiTeamStanding.Points;
                    standing.Form = apiTeamStanding.Form;
                    standing.UpdatedAt = DateTime.UtcNow;

                    standingsUpserted++;
                }
            }

            // Update sync state
            syncState.LastSyncedAt = DateTime.UtcNow;
            syncState.NextAllowedSyncAt = DateTime.UtcNow.Add(StandingsTtl);
            syncState.LastError = null;

            await _db.SaveChangesAsync(ct);

            var result = new SyncGroupStandingsResult
            {
                GroupsCreated = groupsCreated,
                GroupsUpdated = groupsUpdated,
                TeamsAssignedToGroups = teamsAssigned,
                StandingsUpserted = standingsUpserted,
                Warnings = warnings,
                SyncedAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Group standings sync completed for tournament {TournamentId}. " +
                "Groups: {Created} created, {Updated} updated. " +
                "Teams assigned: {TeamsAssigned}. Standings: {Standings} upserted.",
                tournament.Id, groupsCreated, groupsUpdated, teamsAssigned, standingsUpserted);

            return Result<SyncGroupStandingsResult>.Success(result);
        }

        private static string ParseGroupName(string apiGroupName)
        {
            // API-FOOTBALL returns "Group A", "Group B", etc.
            // We want just "A", "B", etc.
            if (string.IsNullOrWhiteSpace(apiGroupName))
                return "Unknown";

            var trimmed = apiGroupName.Trim();

            // Handle "Group A" format
            if (trimmed.StartsWith("Group ", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed.Substring(6).Trim();
            }

            // Handle "Group: A" format
            if (trimmed.StartsWith("Group:", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed.Substring(6).Trim();
            }

            return trimmed;
        }

        private async Task<ExternalSyncState> GetOrCreateSyncState(Guid tournamentId, CancellationToken ct)
        {
            var state = await _db.ExternalSyncStates
                .FirstOrDefaultAsync(s =>
                    s.TournamentId == tournamentId &&
                    s.Provider == Provider &&
                    s.Resource == ResourceStandings, ct);

            if (state == null)
            {
                state = new ExternalSyncState
                {
                    Id = Guid.NewGuid(),
                    TournamentId = tournamentId,
                    Provider = Provider,
                    Resource = ResourceStandings,
                    LastSyncedAt = DateTime.MinValue,
                    NextAllowedSyncAt = DateTime.MinValue,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.ExternalSyncStates.Add(state);
            }

            return state;
        }
    }
}
