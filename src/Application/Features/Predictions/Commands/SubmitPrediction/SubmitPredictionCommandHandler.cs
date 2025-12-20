using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Predictions.Commands.SubmitPrediction
{
    public sealed class SubmitPredictionCommandHandler : IRequestHandler<SubmitPredictionCommand, Result<Guid>>
    {
        private readonly ITipprDbContext _db;
        private readonly ICurrentUser _currentUser;
        private readonly IPointsCalculator _points;

        public SubmitPredictionCommandHandler(ITipprDbContext db, ICurrentUser currentUser, IPointsCalculator points)
        {
            _db = db;
            _currentUser = currentUser;
            _points = points;
        }

        public async Task<Result<Guid>> Handle(SubmitPredictionCommand request, CancellationToken ct)
        {
            var userId = _currentUser.UserId;

            var league = await _db.Leagues
                .Include(l => l.Settings)
                .FirstOrDefaultAsync(l => l.Id == request.LeagueId, ct);

            if (league == null) 
                return Result<Guid>.NotFound("league not found", "league.not_found");

            var match = await _db.Matches.FirstOrDefaultAsync(m => m.Id == request.MatchId, ct);

            if (match == null) 
                return Result<Guid>.NotFound("match not found", "match.not_found");

            if (league.Settings is null)
            {
                return Result<Guid>.Failure(
                    "League settings are missing. This is a system error.",
                    "league.settings_missing"
                );
            }

            var deadlineCheck = EnsureBeforeDeadline(match.MatchDate, league.Settings);
            if (!deadlineCheck.IsSuccess)
            {
                return Result<Guid>.BusinessRule(deadlineCheck.Error!.Message, deadlineCheck.Error.Code);
            }

            var exists = await _db.Predictions.AnyAsync(p =>
                p.UserId == userId &&
                p.LeagueId == request.LeagueId &&
                p.MatchId == request.MatchId, ct);

            if (exists)
            {
                return Result<Guid>.Conflict(
                    "Prediction already exists for this match in this league",
                    "prediction.already_exists"
                );
            }

            var entity = new Prediction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LeagueId = request.LeagueId,
                MatchId = request.MatchId,
                HomeScore = request.HomeScore,
                AwayScore = request.AwayScore,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PointsEarned = 0,
                IsScored = false
            };

            // Points-ready: if match already has result, calculate points immediately
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

            _db.Predictions.Add(entity);
            await _db.SaveChangesAsync(ct);

            return Result<Guid>.Success(entity.Id);
        }

        private static Result EnsureBeforeDeadline(DateTime matchDateUtc, LeagueSettings settings)
        {
            if (settings.AllowLateEdits)
            {
                return Result.Success();
            }

            var deadlineUtc = matchDateUtc.AddMinutes(-settings.DeadlineMinutes);

            if (DateTime.UtcNow > deadlineUtc)
            {
                return Result.BusinessRule("Deadline passed", "prediction.deadline_passed");
            }

            return Result.Success();
        }
    }
}
