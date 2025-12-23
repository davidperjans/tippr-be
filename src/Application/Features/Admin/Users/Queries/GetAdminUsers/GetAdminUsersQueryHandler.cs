using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Users.Queries.GetAdminUsers
{
    public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, Result<PagedResult<AdminUserListDto>>>
    {
        private readonly ITipprDbContext _db;

        public GetAdminUsersQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<PagedResult<AdminUserListDto>>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Users.AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchLower = request.Search.ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(searchLower) ||
                    u.DisplayName.ToLower().Contains(searchLower) ||
                    u.Email.ToLower().Contains(searchLower));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = request.Sort?.ToLower() switch
            {
                "username" => query.OrderBy(u => u.Username),
                "username_desc" => query.OrderByDescending(u => u.Username),
                "email" => query.OrderBy(u => u.Email),
                "email_desc" => query.OrderByDescending(u => u.Email),
                "createdat" => query.OrderBy(u => u.CreatedAt),
                "createdat_desc" => query.OrderByDescending(u => u.CreatedAt),
                "lastloginat" => query.OrderBy(u => u.LastLoginAt),
                "lastloginat_desc" => query.OrderByDescending(u => u.LastLoginAt),
                _ => query.OrderByDescending(u => u.CreatedAt)
            };

            // Apply pagination
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminUserListDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                    AvatarUrl = u.AvatarUrl,
                    Role = u.Role,
                    IsBanned = u.IsBanned,
                    LastLoginAt = u.LastLoginAt,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<AdminUserListDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Result<PagedResult<AdminUserListDto>>.Success(result);
        }
    }
}
