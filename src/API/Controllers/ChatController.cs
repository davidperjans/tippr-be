using API.Contracts.Errors;
using Application.Common;
using Application.Features.Chat.Queries.GetChatMessages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Provides access to league chat messages for real-time communication between members.
    /// </summary>
    /// <remarks>
    /// <para><b>Domain Relationships:</b></para>
    /// <list type="bullet">
    ///   <item><description>Chat messages belong to a League</description></item>
    ///   <item><description>Messages are posted by Users who are league members</description></item>
    ///   <item><description>Messages are deleted when the league is deleted</description></item>
    /// </list>
    ///
    /// <para><b>Note:</b> Message posting is typically handled via SignalR for real-time updates.
    /// This endpoint provides historical message retrieval with cursor-based pagination.</para>
    /// </remarks>
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    [Produces("application/json")]
    public class ChatController : BaseApiController
    {
        private readonly ISender _mediator;

        public ChatController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves chat messages for a league with cursor-based pagination.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Pagination:</b> Uses cursor-based pagination for efficient scrolling through message history</para>
        ///
        /// <para><b>Example Request (first page):</b></para>
        /// <code>
        /// GET /api/chat/messages?leagueId=3fa85f64-5717-4562-b3fc-2c963f66afa6&amp;take=50
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Request (next page):</b></para>
        /// <code>
        /// GET /api/chat/messages?leagueId=3fa85f64-5717-4562-b3fc-2c963f66afa6&amp;cursor=2024-01-15T10:30:00Z&amp;take=50
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "messages": [
        ///       {
        ///         "id": "...",
        ///         "userId": "...",
        ///         "displayName": "John Doe",
        ///         "avatarUrl": "https://...",
        ///         "content": "Great prediction on the Brazil game!",
        ///         "createdAt": "2024-01-15T10:30:00Z"
        ///       },
        ///       {
        ///         "id": "...",
        ///         "userId": "...",
        ///         "displayName": "Jane Smith",
        ///         "avatarUrl": "https://...",
        ///         "content": "Thanks! I had a feeling about that one.",
        ///         "createdAt": "2024-01-15T10:28:00Z"
        ///       }
        ///     ],
        ///     "nextCursor": "2024-01-15T10:25:00Z",
        ///     "hasMore": true
        ///   },
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="leagueId">The league ID to get messages for</param>
        /// <param name="cursor">Optional cursor (CreatedAt timestamp) to fetch messages before this point</param>
        /// <param name="take">Number of messages to retrieve (default: 50, max: 100)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Paginated list of chat messages with cursor for next page</returns>
        /// <response code="200">Returns the chat messages</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User is not a member of the league</response>
        /// <response code="404">League not found</response>
        [HttpGet("messages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMessages(
            [FromQuery] Guid leagueId,
            [FromQuery] DateTime? cursor,
            [FromQuery] int take = 50,
            CancellationToken ct = default)
        {
            var query = new GetChatMessagesQuery(leagueId, cursor, take);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }
    }
}
