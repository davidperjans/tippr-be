using Application.Common;
using Application.Features.Admin.DTOs;
using MediatR;

namespace Application.Features.Admin.Users.Queries.GetAdminUsers
{
    public sealed record GetAdminUsersQuery(
        string? Search,
        int Page = 1,
        int PageSize = 20,
        string? Sort = null
    ) : IRequest<Result<PagedResult<AdminUserListDto>>>;
}
