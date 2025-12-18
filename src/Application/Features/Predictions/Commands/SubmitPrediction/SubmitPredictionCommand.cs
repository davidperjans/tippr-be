using Application.Common;
using MediatR;

namespace Application.Features.Predictions.Commands.SubmitPrediction
{
    public sealed record SubmitPredictionCommand(
        Guid LeagueId,
        Guid MatchId,
        int HomeScore,
        int AwayScore
    ) : IRequest<Result<Guid>>;
}
