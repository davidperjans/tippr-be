using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Predictions.Commands.BulkSubmitPredictions
{
    public sealed class BulkSubmitPredictionsCommandHandler : IRequestHandler<BulkSubmitPredictionsCommand, Result<BulkSubmitPredictionsResult>>
    {
        private readonly ITipprDbContext _db;
        private readonly ICurrentUser _currentUser;
        private readonly IPointsCalculator _points;

        public BulkSubmitPredictionsCommandHandler(ITipprDbContext db, ICurrentUser currentUser, IPointsCalculator points)
        {
            _db = db;
            _currentUser = currentUser;
            _points = points;
        }

        public async Task<Result<BulkSubmitPredictionsResult>> Handle(BulkSubmitPredictionsCommand request, CancellationToken ct)
        {
            var userId = _currentUser.UserId;

            // Get league with settings
            var league = await _db.Leagues
                .Include(l => l.Settings)
                .FirstOrDefaultAsync(l => l.Id == request.LeagueId, ct);

            if (league == null)
                return Result<BulkSubmitPredictionsResult>.NotFound("League not found", "league.not_found");

            if (league.Settings is null)
                return Result<BulkSubmitPredictionsResult>.Failure("League settings are missing", "league.settings_missing");

            // Check if user is a member of the league
            var isMember = await _db.LeagueMembers
                .AnyAsync(lm => lm.LeagueId == request.LeagueId && lm.UserId == userId, ct);

            if (!isMember)
                return Result<BulkSubmitPredictionsResult>.Forbidden("You are not a member of this league", "league.not_member");

            // Get all match IDs from request
            var matchIds = request.Predictions.Select(p => p.MatchId).ToList();

            // Get all matches in one query
            var matches = await _db.Matches
                .Where(m => matchIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, ct);

            // Get existing predictions for these matches
            var existingPredictionsList = await _db.Predictions
                .Where(p => p.UserId == userId && p.LeagueId == request.LeagueId && matchIds.Contains(p.MatchId))
                .Select(p => p.MatchId)
                .ToListAsync(ct);
            var existingPredictions = existingPredictionsList.ToHashSet();

            var results = new List<PredictionResult>();
            var predictionsToAdd = new List<Prediction>();

            foreach (var predictionItem in request.Predictions)
            {
                // Check if match exists
                if (!matches.TryGetValue(predictionItem.MatchId, out var match))
                {
                    results.Add(new PredictionResult
                    {
                        MatchId = predictionItem.MatchId,
                        Success = false,
                        ErrorMessage = "Match not found",
                        ErrorCode = "match.not_found"
                    });
                    continue;
                }

                // Check if prediction already exists
                if (existingPredictions.Contains(predictionItem.MatchId))
                {
                    results.Add(new PredictionResult
                    {
                        MatchId = predictionItem.MatchId,
                        Success = false,
                        ErrorMessage = "Prediction already exists for this match",
                        ErrorCode = "prediction.already_exists"
                    });
                    continue;
                }

                // Check deadline
                if (!league.Settings.AllowLateEdits)
                {
                    var deadlineUtc = match.MatchDate.AddMinutes(-league.Settings.DeadlineMinutes);
                    if (DateTime.UtcNow > deadlineUtc)
                    {
                        results.Add(new PredictionResult
                        {
                            MatchId = predictionItem.MatchId,
                            Success = false,
                            ErrorMessage = "Deadline passed for this match",
                            ErrorCode = "prediction.deadline_passed"
                        });
                        continue;
                    }
                }

                // Validate scores
                if (predictionItem.HomeScore < 0 || predictionItem.AwayScore < 0)
                {
                    results.Add(new PredictionResult
                    {
                        MatchId = predictionItem.MatchId,
                        Success = false,
                        ErrorMessage = "Scores cannot be negative",
                        ErrorCode = "prediction.invalid_score"
                    });
                    continue;
                }

                // Create prediction
                var entity = new Prediction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LeagueId = request.LeagueId,
                    MatchId = predictionItem.MatchId,
                    HomeScore = predictionItem.HomeScore,
                    AwayScore = predictionItem.AwayScore,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    PointsEarned = 0,
                    IsScored = false
                };

                // If match already has result, calculate points immediately
                if (match.HomeScore.HasValue && match.AwayScore.HasValue)
                {
                    entity.PointsEarned = _points.CalculateMatchPoints(
                        predictedHome: entity.HomeScore,
                        predictedAway: entity.AwayScore,
                        actualHome: match.HomeScore.Value,
                        actualAway: match.AwayScore.Value,
                        league.Settings
                    );
                    entity.IsScored = true;
                    entity.ScoredAt = DateTime.UtcNow;
                    entity.ScoredResultVersion = match.ResultVersion;
                }

                predictionsToAdd.Add(entity);
                existingPredictions.Add(predictionItem.MatchId); // Prevent duplicates within same request

                results.Add(new PredictionResult
                {
                    MatchId = predictionItem.MatchId,
                    PredictionId = entity.Id,
                    Success = true
                });
            }

            // Save all predictions at once
            if (predictionsToAdd.Any())
            {
                _db.Predictions.AddRange(predictionsToAdd);
                await _db.SaveChangesAsync(ct);
            }

            return Result<BulkSubmitPredictionsResult>.Success(new BulkSubmitPredictionsResult
            {
                SuccessCount = predictionsToAdd.Count,
                FailedCount = request.Predictions.Count - predictionsToAdd.Count,
                Results = results
            });
        }
    }
}
