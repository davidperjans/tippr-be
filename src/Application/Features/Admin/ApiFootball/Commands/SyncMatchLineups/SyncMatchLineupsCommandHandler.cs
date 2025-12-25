using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Admin.ApiFootball.Commands.SyncMatchLineups
{
    public class SyncMatchLineupsCommandHandler
        : IRequestHandler<SyncMatchLineupsCommand, Result<SyncMatchLineupsResult>>
    {
        private readonly ITipprDbContext _db;
        private readonly IApiFootballClient _apiClient;
        private readonly ILogger<SyncMatchLineupsCommandHandler> _logger;

        // Lineups typically available ~60 minutes before kickoff
        private static readonly TimeSpan LineupsAvailableWindow = TimeSpan.FromMinutes(60);

        public SyncMatchLineupsCommandHandler(
            ITipprDbContext db,
            IApiFootballClient apiClient,
            ILogger<SyncMatchLineupsCommandHandler> logger)
        {
            _db = db;
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<Result<SyncMatchLineupsResult>> Handle(
            SyncMatchLineupsCommand request,
            CancellationToken ct)
        {
            var match = await _db.Matches
                .Include(m => m.Tournament)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == request.MatchId, ct);

            if (match == null)
                return Result<SyncMatchLineupsResult>.NotFound("Match not found", "admin.match_not_found");

            if (!match.Tournament.ApiFootballEnabled)
                return Result<SyncMatchLineupsResult>.BusinessRule(
                    "API-FOOTBALL is not enabled for this tournament", "admin.apifootball_not_enabled");

            if (!match.ApiFootballId.HasValue)
                return Result<SyncMatchLineupsResult>.BusinessRule(
                    "Match does not have an API-FOOTBALL ID", "admin.match_no_apifootball_id");

            var now = DateTime.UtcNow;

            // Check if lineups should be available
            if (!request.Force)
            {
                var isLive = match.Status == MatchStatus.Live;
                var isWithinWindow = match.MatchDate <= now.Add(LineupsAvailableWindow);
                var isRecent = match.MatchDate >= now.AddHours(-6); // Recent finished matches

                if (!isLive && !isWithinWindow && !isRecent)
                {
                    return Result<SyncMatchLineupsResult>.Success(new SyncMatchLineupsResult
                    {
                        Success = false,
                        LineupsAvailable = false,
                        Message = $"Match starts at {match.MatchDate:u}. Lineups typically available 60 minutes before kickoff."
                    });
                }
            }

            // Check for existing recent snapshot
            var existingSnapshot = await _db.MatchLineupSnapshots
                .Where(s => s.MatchId == match.Id)
                .OrderByDescending(s => s.FetchedAt)
                .FirstOrDefaultAsync(ct);

            if (!request.Force && existingSnapshot != null)
            {
                // If we have a recent snapshot, use TTL
                var isLive = match.Status == MatchStatus.Live;
                var ttl = isLive ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(30);

                if (existingSnapshot.FetchedAt > now.Subtract(ttl))
                {
                    return Result<SyncMatchLineupsResult>.Success(new SyncMatchLineupsResult
                    {
                        Success = true,
                        LineupsAvailable = true,
                        TeamsWithLineups = 2, // Assuming we had data
                        FetchedAt = existingSnapshot.FetchedAt,
                        Message = $"Using cached lineups from {existingSnapshot.FetchedAt:u}"
                    });
                }
            }

            // Fetch lineups from API
            var lineupsResult = await _apiClient.GetLineupsAsync(match.ApiFootballId.Value, ct);

            if (!lineupsResult.Success)
            {
                return Result<SyncMatchLineupsResult>.Failure(
                    $"Failed to fetch lineups: {lineupsResult.ErrorMessage}");
            }

            if (lineupsResult.Lineups.Count == 0 || string.IsNullOrWhiteSpace(lineupsResult.RawJson))
            {
                return Result<SyncMatchLineupsResult>.Success(new SyncMatchLineupsResult
                {
                    Success = true,
                    LineupsAvailable = false,
                    TeamsWithLineups = 0,
                    FetchedAt = now,
                    Message = "Lineups not yet available for this match"
                });
            }

            // Store snapshot (replace existing or create new)
            if (existingSnapshot != null)
            {
                existingSnapshot.Json = lineupsResult.RawJson;
                existingSnapshot.FetchedAt = now;
                existingSnapshot.UpdatedAt = now;
            }
            else
            {
                var snapshot = new MatchLineupSnapshot
                {
                    Id = Guid.NewGuid(),
                    MatchId = match.Id,
                    Json = lineupsResult.RawJson,
                    FetchedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _db.MatchLineupSnapshots.Add(snapshot);
            }

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Lineups synced for match {MatchId} ({HomeTeam} vs {AwayTeam}). Teams: {TeamCount}",
                match.Id, match.HomeTeam?.Name, match.AwayTeam?.Name, lineupsResult.Lineups.Count);

            return Result<SyncMatchLineupsResult>.Success(new SyncMatchLineupsResult
            {
                Success = true,
                LineupsAvailable = true,
                TeamsWithLineups = lineupsResult.Lineups.Count,
                FetchedAt = now,
                Message = $"Lineups fetched successfully for {lineupsResult.Lineups.Count} teams"
            });
        }
    }
}
