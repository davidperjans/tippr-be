using API.Contracts.Errors;
using API.Contracts.Users;
using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Users.Commands.UpdateProfile;
using Application.Features.Users.Commands.UploadAvatar;
using Application.Features.Users.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Manages user profile operations including avatar upload and profile updates.
    /// </summary>
    /// <remarks>
    /// All endpoints require authentication. Users can only modify their own profile.
    /// </remarks>
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    public class UsersController : BaseApiController
    {
        private readonly ISender _mediator;
        private readonly ICurrentUser _currentUser;

        public UsersController(ISender mediator, ICurrentUser currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Uploads a new avatar image for the current user.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Content-Type:</b> multipart/form-data</para>
        /// <para><b>Max File Size:</b> 2 MB</para>
        /// <para><b>Supported Formats:</b> JPEG, PNG, GIF, WebP</para>
        /// <para><b>Side Effects:</b> Uploads image to Supabase Storage and updates user's AvatarUrl</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// POST /api/users/avatar
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: multipart/form-data
        ///
        /// file: [binary image data]
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": "https://storage.supabase.co/avatars/user-id/avatar.jpg",
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="request">The avatar file to upload (form field: File)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The public URL of the uploaded avatar image</returns>
        /// <response code="200">Returns the URL of the uploaded avatar</response>
        /// <response code="400">File is missing, too large, or invalid format</response>
        /// <response code="401">JWT token is missing or invalid</response>
        [Authorize]
        [HttpPost("avatar")]
        [RequestSizeLimit(2_000_000)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<string>>> UploadAvatar([FromForm] UploadAvatarRequest request, CancellationToken ct)
        {
            var command = new UploadAvatarCommand(_currentUser.UserId, request.File);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Updates the current user's profile information.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Partial Update:</b> Only provided fields are updated; null/omitted fields are ignored</para>
        ///
        /// <para><b>Field Constraints:</b></para>
        /// <list type="bullet">
        ///   <item><description>displayName: max 100 characters</description></item>
        ///   <item><description>bio: max 500 characters (send empty string to clear)</description></item>
        ///   <item><description>favoriteTeamId: must reference an existing team</description></item>
        /// </list>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// PATCH /api/users/profile
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: application/json
        ///
        /// {
        ///   "displayName": "John Doe",
        ///   "bio": "Football enthusiast since 1990",
        ///   "favoriteTeamId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
        /// }
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "username": "johndoe",
        ///     "displayName": "John Doe",
        ///     "avatarUrl": "https://storage.example.com/avatars/user.jpg",
        ///     "bio": "Football enthusiast since 1990",
        ///     "favoriteTeamId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "favoriteTeamName": "Manchester United",
        ///     "createdAt": "2024-01-01T00:00:00Z",
        ///     "updatedAt": "2024-01-15T10:30:00Z"
        ///   },
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="request">The profile fields to update</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The updated user profile</returns>
        /// <response code="200">Returns the updated user profile</response>
        /// <response code="400">Validation error (e.g., field too long)</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="404">Referenced team does not exist</response>
        [Authorize]
        [HttpPatch("profile")]
        [ProducesResponseType(typeof(Result<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<UserProfileDto>>> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
        {
            var command = new UpdateProfileCommand(
                _currentUser.UserId,
                request.DisplayName,
                request.Bio,
                request.FavoriteTeamId
            );
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
