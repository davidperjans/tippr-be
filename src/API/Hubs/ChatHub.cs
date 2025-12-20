using Application.Common;
using Application.Features.Chat.Commands.SendChatMessage;
using Application.Features.Chat.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ISender _mediator;
        public ChatHub(ISender mediator)
        {
            _mediator = mediator;
        }

        private static string GroupName(Guid leagueId) => $"league:{leagueId}";

        public async Task<Result> JoinLeagueChat(Guid leagueId)
        {
            // Om du vill: gör en mediator-query här som validerar member + !global.
            // MVP: låt SendMessage + history enforce:a, men Join kan vara "öppen".
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(leagueId));
            return Result.Success();
        }

        public async Task<Result> LeaveLeagueChat(Guid leagueId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(leagueId));
            return Result.Success();
        }

        public async Task<Result<ChatMessageDto>> SendMessage(Guid leagueId, string message)
        {
            var result = await _mediator.Send(new SendChatMessageCommand(leagueId, message));

            if (!result.IsSuccess || result.Data is null)
                return result;

            await Clients.Group(GroupName(leagueId)).SendAsync("MessageReceived", result.Data);

            return result;
        }
    }
}
