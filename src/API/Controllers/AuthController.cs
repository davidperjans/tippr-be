using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.Queries.GetMe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Authorize]
    public class AuthController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUser _currentUser;
        public AuthController(IMediator mediator, ICurrentUser currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Hämtar inloggad användares information
        /// User synkas automatiskt via UserSyncMiddleware
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<Result<CurrentUserResponse>>> GetMe()
        {
            var query = new GetCurrentUserQuery(_currentUser.UserId);
            var result = await _mediator.Send(query);
            return FromResult(result);
        }
    }
}
