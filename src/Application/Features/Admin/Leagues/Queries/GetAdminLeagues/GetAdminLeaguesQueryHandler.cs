using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Leagues.Queries.GetAdminLeagues
{
    public class GetAdminLeaguesQueryHandler : IRequestHandler<GetAdminLeaguesQuery, Result<PagedResult<AdminLeagueListDto>>>
    {
        private readonly ITipprDbContext _db;

        public GetAdminLeaguesQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<PagedResult<AdminLeagueListDto>>> Handle(GetAdminLeaguesQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Leagues.AsNoTracking();

            // Apply filters
            if (request.TournamentId.HasValue)
                query = query.Where(l => l.TournamentId == request.TournamentId.Value);

            if (request.OwnerId.HasValue)
                query = query.Where(l => l.OwnerId == request.OwnerId.Value);

            if (request.IsPublic.HasValue)
                query = query.Where(l => l.IsPublic == request.IsPublic.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(l =>
                    l.Name.ToLower().Contains(searchLower) ||
                    (l.Description != null && l.Description.ToLower().Contains(searchLower)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new AdminLeagueListDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    TournamentId = l.TournamentId,
                    TournamentName = l.Tournament.Name,
                    OwnerId = l.OwnerId,
                    OwnerUsername = l.Owner != null ? l.Owner.Username : null,
                    IsPublic = l.IsPublic,
                    IsGlobal = l.IsGlobal,
                    MaxMembers = l.MaxMembers,
                    CreatedAt = l.CreatedAt,
                    MemberCount = l.Members.Count
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<AdminLeagueListDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Result<PagedResult<AdminLeagueListDto>>.Success(result);
        }
    }
}
