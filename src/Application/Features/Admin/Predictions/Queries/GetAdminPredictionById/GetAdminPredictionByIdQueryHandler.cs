using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Predictions.Queries.GetAdminPredictionById
{
    public class GetAdminPredictionByIdQueryHandler : IRequestHandler<GetAdminPredictionByIdQuery, Result<AdminPredictionDto>>
    {
        private readonly ITipprDbContext _db;

        public GetAdminPredictionByIdQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<AdminPredictionDto>> Handle(GetAdminPredictionByIdQuery request, CancellationToken cancellationToken)
        {
            var prediction = await _db.Predictions
                .AsNoTracking()
                .Where(p => p.Id == request.PredictionId)
                .Select(p => new AdminPredictionDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    UserDisplayName = p.User.DisplayName,
                    MatchId = p.MatchId,
                    MatchDescription = p.Match.HomeTeam.Code + " vs " + p.Match.AwayTeam.Code + " (" + p.Match.MatchDate.ToString("yyyy-MM-dd") + ")",
                    LeagueId = p.LeagueId,
                    LeagueName = p.League.Name,
                    HomeScore = p.HomeScore,
                    AwayScore = p.AwayScore,
                    ActualHomeScore = p.Match.HomeScore,
                    ActualAwayScore = p.Match.AwayScore,
                    PointsEarned = p.PointsEarned,
                    IsScored = p.IsScored,
                    ScoredResultVersion = p.ScoredResultVersion,
                    ScoredAt = p.ScoredAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (prediction == null)
                return Result<AdminPredictionDto>.NotFound("Prediction not found", "admin.prediction_not_found");

            return Result<AdminPredictionDto>.Success(prediction);
        }
    }
}
