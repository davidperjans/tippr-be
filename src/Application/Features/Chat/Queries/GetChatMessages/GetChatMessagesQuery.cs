using Application.Common;
using Application.Features.Chat.DTOs;
using MediatR;

namespace Application.Features.Chat.Queries.GetChatMessages
{
    public sealed record GetChatMessagesQuery(
        Guid LeagueId,
        DateTime? Cursor,  // äldre än denna (CreatedAt)
        int Take = 50
    ) : IRequest<Result<GetChatMessagesResponse>>;
}
