using Application.Common;
using Application.Features.Users.DTOs;
using MediatR;

namespace Application.Features.Users.Commands.UpdateProfile
{
    public sealed record UpdateProfileCommand(
        Guid UserId,
        string? DisplayName,
        string? Bio,
        Guid? FavoriteTeamId
    ) : IRequest<Result<UserProfileDto>>;
}
