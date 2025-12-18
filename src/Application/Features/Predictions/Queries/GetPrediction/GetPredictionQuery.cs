using Application.Common;
using Application.Features.Predictions.DTOs;
using MediatR;

namespace Application.Features.Predictions.Queries.GetPrediction
{
    public sealed record GetPredictionQuery(
        Guid LeagueId, 
        Guid MatchId
    ) : IRequest<Result<PredictionDto?>>;
}
