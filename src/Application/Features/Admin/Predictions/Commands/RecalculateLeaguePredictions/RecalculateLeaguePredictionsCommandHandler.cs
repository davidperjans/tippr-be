using Application.Common;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Predictions.Commands.RecalculateLeaguePredictions
{
    public class RecalculateLeaguePredictionsCommandHandler : IRequestHandler<RecalculateLeaguePredictionsCommand, Result<RecalculateLeaguePredictionsResult>>
    {
        private readonly ITipprDbContext _db;
        private readonly IStandingsService _standingsService;

        public RecalculateLeaguePredictionsCommandHandler(ITipprDbContext db, IStandingsService standingsService)
        {
            _db = db;
            _standingsService = standingsService;
        }

        public async Task<Result<RecalculateLeaguePredictionsResult>> Handle(RecalculateLeaguePredictionsCommand request, CancellationToken cancellationToken)
        {
            var league = await _db.Leagues
                .Include(l => l.Settings)
                .FirstOrDefaultAsync(l => l.Id == request.LeagueId, cancellationToken);

            if (league == null)
                return Result<RecalculateLeaguePredictionsResult>.NotFound("League not found", "admin.league_not_found");

            var settings = league.Settings;

            // Get all predictions for finished matches in this league
            var predictions = await _db.Predictions
                .Include(p => p.Match)
                .Where(p => p.LeagueId == request.LeagueId &&
                           p.Match.Status == MatchStatus.FullTime &&
                           p.Match.HomeScore.HasValue &&
                           p.Match.AwayScore.HasValue)
                .ToListAsync(cancellationToken);

            int totalPoints = 0;

            foreach (var prediction in predictions)
            {
                var points = CalculatePoints(
                    prediction.HomeScore,
                    prediction.AwayScore,
                    prediction.Match.HomeScore!.Value,
                    prediction.Match.AwayScore!.Value,
                    settings.PointsCorrectScore,
                    settings.PointsCorrectOutcome,
                    settings.PointsCorrectGoals
                );

                prediction.PointsEarned = points;
                prediction.IsScored = true;
                prediction.ScoredResultVersion = prediction.Match.ResultVersion;
                prediction.ScoredAt = DateTime.UtcNow;
                totalPoints += points;
            }

            await _db.SaveChangesAsync(cancellationToken);

            // Recalculate standings
            await _standingsService.RecalculateRanksForLeagueAsync(request.LeagueId, cancellationToken);

            return Result<RecalculateLeaguePredictionsResult>.Success(new RecalculateLeaguePredictionsResult
            {
                PredictionsUpdated = predictions.Count,
                TotalPoints = totalPoints
            });
        }

        private static int CalculatePoints(
            int predictedHome, int predictedAway,
            int actualHome, int actualAway,
            int correctScorePoints, int correctOutcomePoints, int correctGoalsPoints)
        {
            if (predictedHome == actualHome && predictedAway == actualAway)
                return correctScorePoints;

            int points = 0;

            var predictedOutcome = Math.Sign(predictedHome - predictedAway);
            var actualOutcome = Math.Sign(actualHome - actualAway);
            if (predictedOutcome == actualOutcome)
                points += correctOutcomePoints;

            if (predictedHome == actualHome || predictedAway == actualAway)
                points += correctGoalsPoints;

            return points;
        }
    }
}
