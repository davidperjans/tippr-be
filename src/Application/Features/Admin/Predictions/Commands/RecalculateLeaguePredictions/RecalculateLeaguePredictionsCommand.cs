using Application.Common;
using MediatR;

namespace Application.Features.Admin.Predictions.Commands.RecalculateLeaguePredictions
{
    public sealed record RecalculateLeaguePredictionsCommand(Guid LeagueId) : IRequest<Result<RecalculateLeaguePredictionsResult>>;

    public class RecalculateLeaguePredictionsResult
    {
        public int PredictionsUpdated { get; init; }
        public int TotalPoints { get; init; }
    }
}
