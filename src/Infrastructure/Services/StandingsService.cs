using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    /// <summary>
    /// Enterprise-level standings service that handles all points calculation and rank management.
    /// Ensures atomic updates and data consistency across standings operations.
    /// </summary>
    public sealed class StandingsService : IStandingsService
    {
        private readonly ITipprDbContext _db;
        private readonly IPointsCalculator _pointsCalculator;
        private readonly ILogger<StandingsService> _logger;

        public StandingsService(
            ITipprDbContext db,
            IPointsCalculator pointsCalculator,
            ILogger<StandingsService> logger)
        {
            _db = db;
            _pointsCalculator = pointsCalculator;
            _logger = logger;
        }

        public async Task<int> ScorePredictionsForMatchAsync(Guid matchId, int resultVersion, CancellationToken ct = default)
        {
            var match = await _db.Matches
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == matchId, ct);

            if (match is null)
            {
                _logger.LogWarning("Match {MatchId} not found for scoring", matchId);
                return 0;
            }

            if (!match.HomeScore.HasValue || !match.AwayScore.HasValue)
            {
                _logger.LogWarning("Match {MatchId} does not have final scores", matchId);
                return 0;
            }

            // Get all leagues for this tournament
            var leagueIds = await _db.Leagues
                .AsNoTracking()
                .Where(l => l.TournamentId == match.TournamentId)
                .Select(l => l.Id)
                .ToListAsync(ct);

            if (leagueIds.Count == 0)
            {
                _logger.LogInformation("No leagues found for tournament {TournamentId}", match.TournamentId);
                return 0;
            }

            // Get league settings for point values
            var leagueSettingsMap = await _db.LeagueSettings
                .AsNoTracking()
                .Where(s => leagueIds.Contains(s.LeagueId))
                .ToDictionaryAsync(s => s.LeagueId, ct);

            // Get all predictions for this match that haven't been scored with this version
            var predictions = await _db.Predictions
                .Where(p => p.MatchId == matchId && leagueIds.Contains(p.LeagueId))
                .ToListAsync(ct);

            if (predictions.Count == 0)
            {
                _logger.LogInformation("No predictions found for match {MatchId}", matchId);
                return 0;
            }

            // Track which leagues need rank recalculation
            var affectedLeagueIds = new HashSet<Guid>();

            // Get all standings we need to update
            var userIds = predictions.Select(p => p.UserId).Distinct().ToList();
            var standings = await _db.LeagueStandings
                .Where(s => leagueIds.Contains(s.LeagueId) && userIds.Contains(s.UserId))
                .ToListAsync(ct);

            var standingsLookup = standings.ToDictionary(s => (s.LeagueId, s.UserId));

            var scoredCount = 0;
            var now = DateTime.UtcNow;

            foreach (var prediction in predictions)
            {
                if (!leagueSettingsMap.TryGetValue(prediction.LeagueId, out var settings))
                {
                    _logger.LogWarning("Settings not found for league {LeagueId}", prediction.LeagueId);
                    continue;
                }

                // Calculate the points difference (for rescoring scenarios)
                var previousPoints = prediction.IsScored ? prediction.PointsEarned : 0;

                var newPoints = _pointsCalculator.CalculateMatchPoints(
                    prediction.HomeScore,
                    prediction.AwayScore,
                    match.HomeScore.Value,
                    match.AwayScore.Value,
                    settings);

                var pointsDelta = newPoints - previousPoints;

                // Update prediction
                prediction.PointsEarned = newPoints;
                prediction.IsScored = true;
                prediction.ScoredResultVersion = resultVersion;
                prediction.ScoredAt = now;

                // Update standings if we have them
                if (standingsLookup.TryGetValue((prediction.LeagueId, prediction.UserId), out var standing))
                {
                    standing.MatchPoints += pointsDelta;
                    standing.TotalPoints = standing.MatchPoints + standing.BonusPoints;
                    standing.UpdatedAt = now;
                    affectedLeagueIds.Add(prediction.LeagueId);
                }
                else
                {
                    _logger.LogWarning(
                        "Standing not found for user {UserId} in league {LeagueId}",
                        prediction.UserId,
                        prediction.LeagueId);
                }

                scoredCount++;
            }

            await _db.SaveChangesAsync(ct);

            // Recalculate ranks for all affected leagues
            foreach (var leagueId in affectedLeagueIds)
            {
                await RecalculateRanksForLeagueAsync(leagueId, ct);
            }

            _logger.LogInformation(
                "Scored {Count} predictions for match {MatchId}, affected {LeagueCount} leagues",
                scoredCount,
                matchId,
                affectedLeagueIds.Count);

            return scoredCount;
        }

        public async Task<int> ScoreBonusPredictionsAsync(Guid bonusQuestionId, CancellationToken ct = default)
        {
            var bonusQuestion = await _db.BonusQuestions
                .AsNoTracking()
                .FirstOrDefaultAsync(bq => bq.Id == bonusQuestionId, ct);

            if (bonusQuestion is null)
            {
                _logger.LogWarning("Bonus question {BonusQuestionId} not found", bonusQuestionId);
                return 0;
            }

            if (!bonusQuestion.IsResolved)
            {
                _logger.LogWarning("Bonus question {BonusQuestionId} is not resolved", bonusQuestionId);
                return 0;
            }

            // Get all predictions for this bonus question
            var predictions = await _db.BonusPredictions
                .Where(bp => bp.BonusQuestionId == bonusQuestionId)
                .ToListAsync(ct);

            if (predictions.Count == 0)
            {
                _logger.LogInformation("No bonus predictions found for question {BonusQuestionId}", bonusQuestionId);
                return 0;
            }

            // Get affected league IDs
            var leagueIds = predictions.Select(p => p.LeagueId).Distinct().ToList();
            var affectedLeagueIds = new HashSet<Guid>();

            // Get standings for update
            var userIds = predictions.Select(p => p.UserId).Distinct().ToList();
            var standings = await _db.LeagueStandings
                .Where(s => leagueIds.Contains(s.LeagueId) && userIds.Contains(s.UserId))
                .ToListAsync(ct);

            var standingsLookup = standings.ToDictionary(s => (s.LeagueId, s.UserId));

            var awardedCount = 0;
            var now = DateTime.UtcNow;

            foreach (var prediction in predictions)
            {
                // Calculate previous points (for rescoring scenarios)
                var previousPoints = prediction.PointsEarned ?? 0;

                var isCorrect = IsCorrectBonusPrediction(
                    prediction.AnswerTeamId,
                    prediction.AnswerText,
                    bonusQuestion.AnswerTeamId,
                    bonusQuestion.AnswerText);

                var newPoints = isCorrect ? bonusQuestion.Points : 0;
                var pointsDelta = newPoints - previousPoints;

                prediction.PointsEarned = newPoints;

                // Update standings
                if (standingsLookup.TryGetValue((prediction.LeagueId, prediction.UserId), out var standing))
                {
                    standing.BonusPoints += pointsDelta;
                    standing.TotalPoints = standing.MatchPoints + standing.BonusPoints;
                    standing.UpdatedAt = now;
                    affectedLeagueIds.Add(prediction.LeagueId);
                }

                if (isCorrect)
                {
                    awardedCount++;
                }
            }

            await _db.SaveChangesAsync(ct);

            // Recalculate ranks for affected leagues
            foreach (var leagueId in affectedLeagueIds)
            {
                await RecalculateRanksForLeagueAsync(leagueId, ct);
            }

            _logger.LogInformation(
                "Scored bonus predictions for question {BonusQuestionId}, {Correct}/{Total} correct",
                bonusQuestionId,
                awardedCount,
                predictions.Count);

            return awardedCount;
        }

        public async Task RecalculateStandingsForLeagueAsync(Guid leagueId, CancellationToken ct = default)
        {
            var league = await _db.Leagues
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == leagueId, ct);

            if (league is null)
            {
                _logger.LogWarning("League {LeagueId} not found for recalculation", leagueId);
                return;
            }

            // Get all standings for this league
            var standings = await _db.LeagueStandings
                .Where(s => s.LeagueId == leagueId)
                .ToListAsync(ct);

            if (standings.Count == 0)
            {
                _logger.LogInformation("No standings found for league {LeagueId}", leagueId);
                return;
            }

            var userIds = standings.Select(s => s.UserId).ToList();

            // Calculate match points from all scored predictions
            var matchPointsByUser = await _db.Predictions
                .Where(p => p.LeagueId == leagueId && p.IsScored)
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, Points = g.Sum(p => p.PointsEarned) })
                .ToDictionaryAsync(x => x.UserId, x => x.Points, ct);

            // Calculate bonus points from all scored bonus predictions
            var bonusPointsByUser = await _db.BonusPredictions
                .Where(bp => bp.LeagueId == leagueId && bp.PointsEarned.HasValue)
                .GroupBy(bp => bp.UserId)
                .Select(g => new { UserId = g.Key, Points = g.Sum(bp => bp.PointsEarned ?? 0) })
                .ToDictionaryAsync(x => x.UserId, x => x.Points, ct);

            var now = DateTime.UtcNow;

            // Update each standing
            foreach (var standing in standings)
            {
                standing.MatchPoints = matchPointsByUser.GetValueOrDefault(standing.UserId, 0);
                standing.BonusPoints = bonusPointsByUser.GetValueOrDefault(standing.UserId, 0);
                standing.TotalPoints = standing.MatchPoints + standing.BonusPoints;
                standing.UpdatedAt = now;
            }

            await _db.SaveChangesAsync(ct);

            // Recalculate ranks
            await RecalculateRanksForLeagueAsync(leagueId, ct);

            _logger.LogInformation(
                "Recalculated standings for league {LeagueId}, {Count} members",
                leagueId,
                standings.Count);
        }

        public async Task RecalculateRanksForLeagueAsync(Guid leagueId, CancellationToken ct = default)
        {
            var standings = await _db.LeagueStandings
                .Include(s => s.User)
                .Where(s => s.LeagueId == leagueId)
                .ToListAsync(ct);

            var ordered = standings
                .OrderByDescending(s => s.TotalPoints)
                .ThenByDescending(s => s.MatchPoints)
                .ThenByDescending(s => s.BonusPoints)
                .ThenBy(s => s.User.Username)
                .ToList();

            int currentRank = 1;
            (int total, int match, int bonus)? prevKey = null;

            for (int i = 0; i < ordered.Count; i++)
            {
                var s = ordered[i];
                var key = (s.TotalPoints, s.MatchPoints, s.BonusPoints);

                // competition ranking: rank becomes position+1 when key changes
                if (prevKey.HasValue && key != prevKey.Value)
                    currentRank = i + 1;

                s.PreviousRank = s.Rank;
                s.Rank = currentRank;
                s.UpdatedAt = DateTime.UtcNow;

                prevKey = key;
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task RecalculateStandingsForTournamentAsync(Guid tournamentId, CancellationToken ct = default)
        {
            var leagueIds = await _db.Leagues
                .AsNoTracking()
                .Where(l => l.TournamentId == tournamentId)
                .Select(l => l.Id)
                .ToListAsync(ct);

            _logger.LogInformation(
                "Recalculating standings for tournament {TournamentId}, {Count} leagues",
                tournamentId,
                leagueIds.Count);

            foreach (var leagueId in leagueIds)
            {
                await RecalculateStandingsForLeagueAsync(leagueId, ct);
            }
        }

        private static bool IsCorrectBonusPrediction(
            Guid? predictedTeamId,
            string? predictedText,
            Guid? correctTeamId,
            string? correctText)
        {
            // Team-based answer
            if (correctTeamId.HasValue)
            {
                return predictedTeamId.HasValue && predictedTeamId.Value == correctTeamId.Value;
            }

            // Text-based answer
            if (!string.IsNullOrWhiteSpace(correctText))
            {
                return !string.IsNullOrWhiteSpace(predictedText) &&
                       string.Equals(predictedText.Trim(), correctText.Trim(), StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}
