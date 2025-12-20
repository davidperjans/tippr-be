using Application.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Users.Commands.UploadAvatar
{

    public sealed record UploadAvatarCommand(
        Guid UserId, 
        IFormFile File
    ) : IRequest<Result<string>>;
}
