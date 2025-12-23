using Application.Common;
using MediatR;

namespace Application.Features.Predictions.Commands.BulkSubmitPredictions
{
    public sealed record BulkSubmitPredictionsCommand(
        Guid LeagueId,
        List<PredictionItem> Predictions
    ) : IRequest<Result<BulkSubmitPredictionsResult>>;

    public sealed record PredictionItem(
        Guid MatchId,
        int HomeScore,
        int AwayScore
    );

    public class BulkSubmitPredictionsResult
    {
        public int SuccessCount { get; init; }
        public int FailedCount { get; init; }
        public List<PredictionResult> Results { get; init; } = new();
    }

    public class PredictionResult
    {
        public Guid MatchId { get; init; }
        public Guid? PredictionId { get; init; }
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public string? ErrorCode { get; init; }
    }
}
