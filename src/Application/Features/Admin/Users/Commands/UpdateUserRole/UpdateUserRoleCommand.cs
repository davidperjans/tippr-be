using Application.Common;
using Domain.Enums;
using MediatR;

namespace Application.Features.Admin.Users.Commands.UpdateUserRole
{
    public sealed record UpdateUserRoleCommand(
        Guid UserId,
        UserRole Role
    ) : IRequest<Result<bool>>;
}
