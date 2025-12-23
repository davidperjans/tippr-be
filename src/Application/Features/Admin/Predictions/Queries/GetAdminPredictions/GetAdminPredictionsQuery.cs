using Application.Common;
using Application.Features.Admin.DTOs;
using MediatR;

namespace Application.Features.Admin.Predictions.Queries.GetAdminPredictions
{
    public sealed record GetAdminPredictionsQuery(
        Guid? LeagueId,
        Guid? MatchId,
        Guid? UserId,
        Guid? TournamentId,
        int Page = 1,
        int PageSize = 20
    ) : IRequest<Result<PagedResult<AdminPredictionListDto>>>;
}
