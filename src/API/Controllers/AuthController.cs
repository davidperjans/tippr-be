using API.Contracts.Errors;
using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.Queries.GetMe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Handles user authentication and session management.
    /// Users are automatically synced from Supabase Auth via the UserSyncMiddleware on first authenticated request.
    /// </summary>
    /// <remarks>
    /// <para><b>Authentication Flow</b></para>
    /// <para>1. User authenticates with Supabase Auth (client-side)</para>
    /// <para>2. Client includes JWT token in Authorization header</para>
    /// <para>3. UserSyncMiddleware automatically creates/updates user in local database</para>
    /// <para>4. Call GET /auth/me to retrieve current user profile</para>
    /// </remarks>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
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
        /// Retrieves the current authenticated user's profile information.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Side Effects:</b> Updates the user's LastLoginAt timestamp</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/auth/me
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "email": "user@example.com",
        ///     "username": "johndoe",
        ///     "displayName": "John Doe",
        ///     "avatarUrl": "https://storage.example.com/avatars/user.jpg",
        ///     "bio": "Football enthusiast",
        ///     "favoriteTeamId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "favoriteTeamName": "Manchester United",
        ///     "lastLoginAt": "2024-01-15T10:30:00Z",
        ///     "role": "User"
        ///   },
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <returns>The current user's profile including username, display name, avatar, bio, and favorite team.</returns>
        /// <response code="200">Returns the current user's profile</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="404">User has not been synced (should not occur with middleware)</response>
        [Authorize(AuthenticationSchemes = "SupabaseAuth")]
        [HttpGet("me")]
        [ProducesResponseType(typeof(Result<CurrentUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<CurrentUserResponse>>> GetMe()
        {
            var query = new GetCurrentUserQuery(_currentUser.AuthUserId);
            var result = await _mediator.Send(query);
            return FromResult(result);
        }
    }
}
