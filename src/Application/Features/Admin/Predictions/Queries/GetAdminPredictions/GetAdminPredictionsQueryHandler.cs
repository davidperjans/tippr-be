using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Predictions.Queries.GetAdminPredictions
{
    public class GetAdminPredictionsQueryHandler : IRequestHandler<GetAdminPredictionsQuery, Result<PagedResult<AdminPredictionListDto>>>
    {
        private readonly ITipprDbContext _db;

        public GetAdminPredictionsQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<PagedResult<AdminPredictionListDto>>> Handle(GetAdminPredictionsQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Predictions.AsNoTracking();

            if (request.LeagueId.HasValue)
                query = query.Where(p => p.LeagueId == request.LeagueId.Value);

            if (request.MatchId.HasValue)
                query = query.Where(p => p.MatchId == request.MatchId.Value);

            if (request.UserId.HasValue)
                query = query.Where(p => p.UserId == request.UserId.Value);

            if (request.TournamentId.HasValue)
                query = query.Where(p => p.Match.TournamentId == request.TournamentId.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new AdminPredictionListDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    MatchId = p.MatchId,
                    MatchDescription = p.Match.HomeTeam.Code + " vs " + p.Match.AwayTeam.Code,
                    LeagueId = p.LeagueId,
                    LeagueName = p.League.Name,
                    HomeScore = p.HomeScore,
                    AwayScore = p.AwayScore,
                    PointsEarned = p.PointsEarned,
                    IsScored = p.IsScored
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<AdminPredictionListDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Result<PagedResult<AdminPredictionListDto>>.Success(result);
        }
    }
}
