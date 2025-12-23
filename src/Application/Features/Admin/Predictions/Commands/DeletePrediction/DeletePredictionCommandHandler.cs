using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Predictions.Commands.DeletePrediction
{
    public class DeletePredictionCommandHandler : IRequestHandler<DeletePredictionCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;
        private readonly IStandingsService _standingsService;

        public DeletePredictionCommandHandler(ITipprDbContext db, IStandingsService standingsService)
        {
            _db = db;
            _standingsService = standingsService;
        }

        public async Task<Result<bool>> Handle(DeletePredictionCommand request, CancellationToken cancellationToken)
        {
            var prediction = await _db.Predictions
                .FirstOrDefaultAsync(p => p.Id == request.PredictionId, cancellationToken);

            if (prediction == null)
                return Result<bool>.NotFound("Prediction not found", "admin.prediction_not_found");

            var leagueId = prediction.LeagueId;

            _db.Predictions.Remove(prediction);
            await _db.SaveChangesAsync(cancellationToken);

            // Recalculate standings for the league
            await _standingsService.RecalculateRanksForLeagueAsync(leagueId, cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
