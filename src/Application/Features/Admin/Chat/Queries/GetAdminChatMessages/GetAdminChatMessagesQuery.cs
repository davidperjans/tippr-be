using Application.Common;
using Application.Features.Admin.DTOs;
using MediatR;

namespace Application.Features.Admin.Chat.Queries.GetAdminChatMessages
{
    public sealed record GetAdminChatMessagesQuery(
        Guid? LeagueId,
        DateTime? Cursor,
        int Take = 50
    ) : IRequest<Result<AdminChatMessagesResponse>>;
}
