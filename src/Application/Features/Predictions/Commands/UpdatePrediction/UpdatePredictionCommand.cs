using Application.Common;
using MediatR;

namespace Application.Features.Predictions.Commands.UpdatePrediction
{
    public sealed record UpdatePredictionCommand(
        Guid PredictionId,
        int HomeScore,
        int AwayScore
    ) : IRequest<Result<bool>>;
}
