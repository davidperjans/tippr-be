using API.Contracts.Users;
using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Users.Commands.UploadAvatar;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : BaseApiController
    {
        private readonly ISender _mediator;
        private readonly ICurrentUser _currentUser;
        public UsersController(ISender mediator, ICurrentUser currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        [Authorize]
        [HttpPost("avatar")]
        [RequestSizeLimit(2_000_000)] // 2 MB extra safety
        public async Task<ActionResult<Result<string>>> UploadAvatar([FromForm] UploadAvatarRequest request, CancellationToken ct)
        {
            var command = new UploadAvatarCommand(_currentUser.UserId, request.File);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
