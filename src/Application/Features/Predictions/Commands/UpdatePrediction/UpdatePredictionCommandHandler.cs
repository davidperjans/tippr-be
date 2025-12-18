using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Predictions.Commands.UpdatePrediction
{
    public sealed class UpdatePredictionCommandHandler : IRequestHandler<UpdatePredictionCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;
        private readonly ICurrentUser _currentUser;
        private readonly IPointsCalculator _points;

        public UpdatePredictionCommandHandler(ITipprDbContext db, ICurrentUser currentUser, IPointsCalculator points)
        {
            _db = db;
            _currentUser = currentUser;
            _points = points;
        }

        public async Task<Result<bool>> Handle(UpdatePredictionCommand request, CancellationToken ct)
        {
            var userId = _currentUser.UserId;

            var prediction = await _db.Predictions.FirstOrDefaultAsync(p => p.Id == request.PredictionId, ct);

            if (prediction == null) 
                return Result<bool>.NotFound("prediction not found", "prediction.not_found");
            
            if (prediction.UserId != userId) 
                return Result<bool>.Forbidden("user is not the predicter", "prediction.forbidden");

            var league = await _db.Leagues.Include(l => l.Settings)
                .FirstOrDefaultAsync(l => l.Id == prediction.LeagueId, ct);
            
            if (league == null) 
                return Result<bool>.NotFound("league not found", "league.not_found");

            var match = await _db.Matches.FirstOrDefaultAsync(m => m.Id == prediction.MatchId, ct);
            
            if (match == null) 
                return Result<bool>.NotFound("match not found", "match.not_found");

            // block efter deadline om AllowLateEdits=false
            var deadlineUtc = match.MatchDate.AddMinutes(-league.Settings.DeadlineMinutes);

            if (!league.Settings.AllowLateEdits && DateTime.UtcNow > deadlineUtc)
            {
                return Result<bool>.BusinessRule("deadline passed. Prediction can no longer be updated.", "prediction.deadline_passed");
            }

            prediction.HomeScore = request.HomeScore;
            prediction.AwayScore = request.AwayScore;
            prediction.UpdatedAt = DateTime.UtcNow;

            // points-ready: om matchen redan har resultat, uppdatera points direkt
            if (match.HomeScore.HasValue && match.AwayScore.HasValue)
            {
                prediction.PointsEarned = _points.CalculateMatchPoints(
                    prediction.HomeScore, prediction.AwayScore,
                    match.HomeScore.Value, match.AwayScore.Value,
                    league.Settings
                );
            }

            await _db.SaveChangesAsync(ct);
            return Result<bool>.Success(true);
        }
    }
}
