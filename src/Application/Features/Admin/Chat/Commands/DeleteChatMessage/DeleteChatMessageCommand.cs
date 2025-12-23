using Application.Common;
using MediatR;

namespace Application.Features.Admin.Chat.Commands.DeleteChatMessage
{
    public sealed record DeleteChatMessageCommand(Guid MessageId) : IRequest<Result<bool>>;
}
