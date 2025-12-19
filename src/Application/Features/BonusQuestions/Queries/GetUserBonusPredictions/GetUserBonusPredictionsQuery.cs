using Application.Common;
using Application.Features.Predictions.DTOs;
using MediatR;

namespace Application.Features.BonusQuestions.Queries.GetUserBonusPredictions
{
    public sealed record GetUserBonusPredictionsQuery(
        Guid LeagueId
    ) : IRequest<Result<List<BonusPredictionDto>>>;
}
