using Application.Common;
using Application.Features.Admin.Chat.Commands.DeleteChatMessage;
using Application.Features.Admin.Chat.Queries.GetAdminChatMessages;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/chat")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminChatController : BaseApiController
    {
        private readonly ISender _mediator;

        public AdminChatController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("messages")]
        public async Task<ActionResult<Result<AdminChatMessagesResponse>>> GetChatMessages(
            [FromQuery] Guid? leagueId,
            [FromQuery] DateTime? cursor,
            [FromQuery] int take = 50,
            CancellationToken ct = default)
        {
            var query = new GetAdminChatMessagesQuery(leagueId, cursor, take);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        [HttpDelete("messages/{messageId:guid}")]
        public async Task<ActionResult<Result<bool>>> DeleteChatMessage(
            Guid messageId,
            CancellationToken ct = default)
        {
            var command = new DeleteChatMessageCommand(messageId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
