using Application.Features.Chat.Queries.GetChatMessages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : BaseApiController
    {
        private readonly ISender _mediator;
        public ChatController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("messages")]
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
