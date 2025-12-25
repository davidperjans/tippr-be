using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Admin.ApiFootball.Commands.SyncTeamSquads
{
    public class SyncTeamSquadsCommandHandler
        : IRequestHandler<SyncTeamSquadsCommand, Result<SyncTeamSquadsResult>>
    {
        private readonly ITipprDbContext _db;
        private readonly IApiFootballClient _apiClient;
        private readonly ILogger<SyncTeamSquadsCommandHandler> _logger;

        private const string Provider = "ApiFootball";
        private const string ResourceSquads = "Squads";
        private static readonly TimeSpan SquadsTtl = TimeSpan.FromHours(24);

        public SyncTeamSquadsCommandHandler(
            ITipprDbContext db,
            IApiFootballClient apiClient,
            ILogger<SyncTeamSquadsCommandHandler> logger)
        {
            _db = db;
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<Result<SyncTeamSquadsResult>> Handle(
            SyncTeamSquadsCommand request,
            CancellationToken ct)
        {
            var tournament = await _db.Tournaments
                .FirstOrDefaultAsync(t => t.Id == request.TournamentId, ct);

            if (tournament == null)
                return Result<SyncTeamSquadsResult>.NotFound("Tournament not found", "admin.tournament_not_found");

            if (!tournament.ApiFootballEnabled)
                return Result<SyncTeamSquadsResult>.BusinessRule(
                    "API-FOOTBALL is not enabled for this tournament", "admin.apifootball_not_enabled");

            if (!tournament.ApiFootballSeason.HasValue)
                return Result<SyncTeamSquadsResult>.BusinessRule(
                    "API-FOOTBALL season is not configured for this tournament", "admin.apifootball_season_not_set");

            // Check TTL for squads sync
            var syncState = await GetOrCreateSyncState(tournament.Id, ct);
            if (!request.Force && syncState.NextAllowedSyncAt > DateTime.UtcNow)
            {
                _logger.LogInformation(
                    "Skipping squads sync - TTL not expired. Next allowed: {NextAllowed}",
                    syncState.NextAllowedSyncAt);

                return Result<SyncTeamSquadsResult>.Success(new SyncTeamSquadsResult
                {
                    SyncedAt = syncState.LastSyncedAt,
                    Warnings = new List<string> { "Sync skipped - TTL not expired" }
                });
            }

            // Get all teams with ApiFootballId
            var teams = await _db.Teams
                .Where(t => t.TournamentId == tournament.Id && t.ApiFootballId.HasValue)
                .ToListAsync(ct);

            if (!teams.Any())
            {
                return Result<SyncTeamSquadsResult>.BusinessRule(
                    "No teams with ApiFootballId found. Run baseline sync first.",
                    "admin.no_teams_mapped");
            }

            var warnings = new List<string>();
            var teamsProcessed = 0;
            var teamsSkipped = 0;
            var playersCreated = 0;
            var playersUpdated = 0;

            // Load existing players for this tournament's teams
            var teamIds = teams.Select(t => t.Id).ToList();
            var existingPlayers = await _db.Players
                .Where(p => teamIds.Contains(p.TeamId))
                .ToListAsync(ct);

            var playersByTeam = existingPlayers
                .GroupBy(p => p.TeamId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var season = tournament.ApiFootballSeason!.Value;

            foreach (var team in teams)
            {
                try
                {
                    var playersResult = await _apiClient.GetPlayersAsync(team.ApiFootballId!.Value, season, ct);

                    if (!playersResult.Success)
                    {
                        warnings.Add($"Failed to fetch players for {team.Name}: {playersResult.ErrorMessage}");
                        teamsSkipped++;
                        continue;
                    }

                    var teamPlayers = playersByTeam.GetValueOrDefault(team.Id) ?? new List<Player>();

                    foreach (var apiPlayer in playersResult.Players)
                    {
                        // Find existing player by ApiFootballId
                        var existingPlayer = teamPlayers.FirstOrDefault(p =>
                            p.ApiFootballId.HasValue && p.ApiFootballId.Value == apiPlayer.ApiFootballId);

                        if (existingPlayer != null)
                        {
                            // Update existing player with all detailed fields
                            existingPlayer.Name = apiPlayer.Name;
                            existingPlayer.FirstName = apiPlayer.FirstName;
                            existingPlayer.LastName = apiPlayer.LastName;
                            existingPlayer.Number = apiPlayer.Number;
                            existingPlayer.Position = apiPlayer.Position;
                            existingPlayer.PhotoUrl = apiPlayer.PhotoUrl;
                            existingPlayer.Age = apiPlayer.Age;
                            existingPlayer.DateOfBirth = apiPlayer.DateOfBirth;
                            existingPlayer.Nationality = apiPlayer.Nationality;
                            existingPlayer.Height = apiPlayer.Height;
                            existingPlayer.Weight = apiPlayer.Weight;
                            existingPlayer.Injured = apiPlayer.Injured;
                            existingPlayer.UpdatedAt = DateTime.UtcNow;
                            playersUpdated++;
                        }
                        else
                        {
                            // Create new player with all detailed fields
                            var newPlayer = new Player
                            {
                                Id = Guid.NewGuid(),
                                TeamId = team.Id,
                                ApiFootballId = apiPlayer.ApiFootballId,
                                Name = apiPlayer.Name,
                                FirstName = apiPlayer.FirstName,
                                LastName = apiPlayer.LastName,
                                Number = apiPlayer.Number,
                                Position = apiPlayer.Position,
                                PhotoUrl = apiPlayer.PhotoUrl,
                                Age = apiPlayer.Age,
                                DateOfBirth = apiPlayer.DateOfBirth,
                                Nationality = apiPlayer.Nationality,
                                Height = apiPlayer.Height,
                                Weight = apiPlayer.Weight,
                                Injured = apiPlayer.Injured,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            _db.Players.Add(newPlayer);
                            teamPlayers.Add(newPlayer);
                            playersCreated++;
                        }
                    }

                    teamsProcessed++;

                    _logger.LogInformation(
                        "Synced players for team {TeamName}: {PlayerCount} players",
                        team.Name, playersResult.Players.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing players for team {TeamName}", team.Name);
                    warnings.Add($"Error syncing {team.Name}: {ex.Message}");
                    teamsSkipped++;
                }
            }

            // Update sync state
            syncState.LastSyncedAt = DateTime.UtcNow;
            syncState.NextAllowedSyncAt = DateTime.UtcNow.Add(SquadsTtl);
            syncState.LastError = null;

            await _db.SaveChangesAsync(ct);

            var result = new SyncTeamSquadsResult
            {
                TeamsProcessed = teamsProcessed,
                TeamsSkipped = teamsSkipped,
                PlayersCreated = playersCreated,
                PlayersUpdated = playersUpdated,
                Warnings = warnings,
                SyncedAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Team squads sync completed for tournament {TournamentId}. " +
                "Teams: {Processed} processed, {Skipped} skipped. " +
                "Players: {Created} created, {Updated} updated.",
                tournament.Id, teamsProcessed, teamsSkipped, playersCreated, playersUpdated);

            return Result<SyncTeamSquadsResult>.Success(result);
        }

        private async Task<ExternalSyncState> GetOrCreateSyncState(Guid tournamentId, CancellationToken ct)
        {
            var state = await _db.ExternalSyncStates
                .FirstOrDefaultAsync(s =>
                    s.TournamentId == tournamentId &&
                    s.Provider == Provider &&
                    s.Resource == ResourceSquads, ct);

            if (state == null)
            {
                state = new ExternalSyncState
                {
                    Id = Guid.NewGuid(),
                    TournamentId = tournamentId,
                    Provider = Provider,
                    Resource = ResourceSquads,
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
