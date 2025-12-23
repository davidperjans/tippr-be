using Application.Common;
using Application.Features.Admin.DTOs;
using MediatR;

namespace Application.Features.Admin.Users.Queries.GetAdminUserById
{
    public sealed record GetAdminUserByIdQuery(Guid UserId) : IRequest<Result<AdminUserDto>>;
}
