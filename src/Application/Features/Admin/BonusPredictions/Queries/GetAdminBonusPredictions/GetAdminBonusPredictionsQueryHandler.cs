using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.BonusPredictions.Queries.GetAdminBonusPredictions
{
    public class GetAdminBonusPredictionsQueryHandler : IRequestHandler<GetAdminBonusPredictionsQuery, Result<PagedResult<AdminBonusPredictionListDto>>>
    {
        private readonly ITipprDbContext _db;

        public GetAdminBonusPredictionsQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<PagedResult<AdminBonusPredictionListDto>>> Handle(GetAdminBonusPredictionsQuery request, CancellationToken cancellationToken)
        {
            var query = _db.BonusPredictions.AsNoTracking();

            if (request.LeagueId.HasValue)
                query = query.Where(bp => bp.LeagueId == request.LeagueId.Value);

            if (request.QuestionId.HasValue)
                query = query.Where(bp => bp.BonusQuestionId == request.QuestionId.Value);

            if (request.UserId.HasValue)
                query = query.Where(bp => bp.UserId == request.UserId.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var items = await query
                .OrderByDescending(bp => bp.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(bp => new AdminBonusPredictionListDto
                {
                    Id = bp.Id,
                    UserId = bp.UserId,
                    Username = bp.User.Username,
                    BonusQuestionId = bp.BonusQuestionId,
                    LeagueId = bp.LeagueId,
                    LeagueName = bp.League.Name,
                    AnswerTeamId = bp.AnswerTeamId,
                    AnswerTeamName = bp.AnswerTeam != null ? bp.AnswerTeam.Name : null,
                    AnswerText = bp.AnswerText,
                    PointsEarned = bp.PointsEarned
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<AdminBonusPredictionListDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Result<PagedResult<AdminBonusPredictionListDto>>.Success(result);
        }
    }
}
