using Application.Common;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Matches.Commands.RecalculateMatchPoints
{
    public class RecalculateMatchPointsCommandHandler : IRequestHandler<RecalculateMatchPointsCommand, Result<RecalculateMatchPointsResult>>
    {
        private readonly ITipprDbContext _db;
        private readonly IStandingsService _standingsService;

        public RecalculateMatchPointsCommandHandler(ITipprDbContext db, IStandingsService standingsService)
        {
            _db = db;
            _standingsService = standingsService;
        }

        public async Task<Result<RecalculateMatchPointsResult>> Handle(RecalculateMatchPointsCommand request, CancellationToken cancellationToken)
        {
            var match = await _db.Matches
                .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

            if (match == null)
                return Result<RecalculateMatchPointsResult>.NotFound("Match not found", "admin.match_not_found");

            if (match.Status != MatchStatus.FullTime || !match.HomeScore.HasValue || !match.AwayScore.HasValue)
                return Result<RecalculateMatchPointsResult>.BusinessRule("Match must be finished with scores to recalculate points", "admin.match_not_finished");

            // Get all predictions for this match
            var predictions = await _db.Predictions
                .Include(p => p.League)
                    .ThenInclude(l => l.Settings)
                .Where(p => p.MatchId == request.MatchId)
                .ToListAsync(cancellationToken);

            var affectedLeagueIds = new HashSet<Guid>();

            foreach (var prediction in predictions)
            {
                var settings = prediction.League.Settings;
                var points = CalculatePoints(
                    prediction.HomeScore,
                    prediction.AwayScore,
                    match.HomeScore.Value,
                    match.AwayScore.Value,
                    settings.PointsCorrectScore,
                    settings.PointsCorrectOutcome,
                    settings.PointsCorrectGoals
                );

                prediction.PointsEarned = points;
                prediction.IsScored = true;
                prediction.ScoredResultVersion = match.ResultVersion;
                prediction.ScoredAt = DateTime.UtcNow;

                affectedLeagueIds.Add(prediction.LeagueId);
            }

            await _db.SaveChangesAsync(cancellationToken);

            // Recalculate standings for all affected leagues
            foreach (var leagueId in affectedLeagueIds)
            {
                await _standingsService.RecalculateRanksForLeagueAsync(leagueId, cancellationToken);
            }

            return Result<RecalculateMatchPointsResult>.Success(new RecalculateMatchPointsResult
            {
                PredictionsUpdated = predictions.Count,
                LeaguesAffected = affectedLeagueIds.Count
            });
        }

        private static int CalculatePoints(
            int predictedHome, int predictedAway,
            int actualHome, int actualAway,
            int correctScorePoints, int correctOutcomePoints, int correctGoalsPoints)
        {
            // Exact score match
            if (predictedHome == actualHome && predictedAway == actualAway)
                return correctScorePoints;

            int points = 0;

            // Correct outcome (win/draw/loss)
            var predictedOutcome = Math.Sign(predictedHome - predictedAway);
            var actualOutcome = Math.Sign(actualHome - actualAway);
            if (predictedOutcome == actualOutcome)
                points += correctOutcomePoints;

            // Correct number of goals for one team
            if (predictedHome == actualHome || predictedAway == actualAway)
                points += correctGoalsPoints;

            return points;
        }
    }
}
