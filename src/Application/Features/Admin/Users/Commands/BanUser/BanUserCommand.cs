using Application.Common;
using MediatR;

namespace Application.Features.Admin.Users.Commands.BanUser
{
    public sealed record BanUserCommand(Guid UserId) : IRequest<Result<bool>>;
}
