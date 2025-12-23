using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Chat.Queries.GetAdminChatMessages
{
    public class GetAdminChatMessagesQueryHandler : IRequestHandler<GetAdminChatMessagesQuery, Result<AdminChatMessagesResponse>>
    {
        private readonly ITipprDbContext _db;

        public GetAdminChatMessagesQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<AdminChatMessagesResponse>> Handle(GetAdminChatMessagesQuery request, CancellationToken cancellationToken)
        {
            var query = _db.ChatMessages.AsNoTracking();

            if (request.LeagueId.HasValue)
                query = query.Where(cm => cm.LeagueId == request.LeagueId.Value);

            if (request.Cursor.HasValue)
                query = query.Where(cm => cm.CreatedAt < request.Cursor.Value);

            var take = Math.Clamp(request.Take, 1, 100);

            var messages = await query
                .OrderByDescending(cm => cm.CreatedAt)
                .Take(take + 1) // Take one extra to check if there are more
                .Select(cm => new AdminChatMessageDto
                {
                    Id = cm.Id,
                    LeagueId = cm.LeagueId,
                    LeagueName = cm.League.Name,
                    UserId = cm.UserId,
                    Username = cm.User.Username,
                    UserDisplayName = cm.User.DisplayName,
                    UserAvatarUrl = cm.User.AvatarUrl,
                    Message = cm.Message,
                    IsEdited = cm.IsEdited,
                    EditedAt = cm.EditedAt,
                    IsDeleted = cm.IsDeleted,
                    CreatedAt = cm.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var hasMore = messages.Count > take;
            if (hasMore)
                messages = messages.Take(take).ToList();

            var response = new AdminChatMessagesResponse
            {
                Messages = messages,
                NextCursor = messages.Any() ? messages.Last().CreatedAt : null,
                HasMore = hasMore
            };

            return Result<AdminChatMessagesResponse>.Success(response);
        }
    }
}
