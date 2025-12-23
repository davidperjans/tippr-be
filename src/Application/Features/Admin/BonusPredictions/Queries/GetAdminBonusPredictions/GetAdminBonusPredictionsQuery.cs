using Application.Common;
using Application.Features.Admin.DTOs;
using MediatR;

namespace Application.Features.Admin.BonusPredictions.Queries.GetAdminBonusPredictions
{
    public sealed record GetAdminBonusPredictionsQuery(
        Guid? LeagueId,
        Guid? QuestionId,
        Guid? UserId,
        int Page = 1,
        int PageSize = 20
    ) : IRequest<Result<PagedResult<AdminBonusPredictionListDto>>>;
}
