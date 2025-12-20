using Application.Common;
using Application.Features.Chat.DTOs;
using MediatR;

namespace Application.Features.Chat.Commands.SendChatMessage
{
    public sealed record SendChatMessageCommand(
        Guid LeagueId,
        string Message
    ) : IRequest<Result<ChatMessageDto>>;
}
