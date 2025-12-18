using Application.Common;
using Application.Features.Predictions.DTOs;
using MediatR;

namespace Application.Features.Predictions.Queries.GetUserPredictions
{
    public sealed record GetUserPredictionsQuery(
        Guid LeagueId
    ) : IRequest<Result<List<PredictionDto>>>;
}
