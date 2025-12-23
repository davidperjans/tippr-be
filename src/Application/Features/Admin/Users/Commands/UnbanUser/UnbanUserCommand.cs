using Application.Common;
using MediatR;

namespace Application.Features.Admin.Users.Commands.UnbanUser
{
    public sealed record UnbanUserCommand(Guid UserId) : IRequest<Result<bool>>;
}
