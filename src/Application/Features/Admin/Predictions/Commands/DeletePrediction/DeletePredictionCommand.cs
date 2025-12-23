using Application.Common;
using MediatR;

namespace Application.Features.Admin.Predictions.Commands.DeletePrediction
{
    public sealed record DeletePredictionCommand(Guid PredictionId) : IRequest<Result<bool>>;
}
