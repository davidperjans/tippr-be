using Application.Common;
using Application.Features.Admin.DTOs;
using MediatR;

namespace Application.Features.Admin.Users.Commands.UpdateAdminUser
{
    public sealed record UpdateAdminUserCommand(
        Guid UserId,
        string? Username,
        string? DisplayName,
        string? Email,
        string? Bio,
        string? AvatarUrl
    ) : IRequest<Result<AdminUserDto>>;
}
