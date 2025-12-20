using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Chat.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Chat.Queries.GetChatMessages
{
    public sealed class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, Result<GetChatMessagesResponse>>
    {
        private readonly ITipprDbContext _db;
        private readonly ICurrentUser _currentUser;

        public GetChatMessagesQueryHandler(ITipprDbContext db, ICurrentUser currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<Result<GetChatMessagesResponse>> Handle(GetChatMessagesQuery request, CancellationToken ct)
        {
            var userId = _currentUser.UserId;

            // 1) League exists + not global
            var league = await _db.Leagues
                .AsNoTracking()
                .Where(x => x.Id == request.LeagueId)
                .Select(x => new { x.Id, x.IsGlobal })
                .FirstOrDefaultAsync(ct);

            if (league is null)
                return Result<GetChatMessagesResponse>.NotFound("league not found", "league.not_found");

            if (league.IsGlobal)
                return Result<GetChatMessagesResponse>.BusinessRule("chat is not enabled for global leagues", "chat.not_enabled_for_global_leagues");

            // 2) Membership check
            var isMember = await _db.LeagueMembers
                .AsNoTracking()
                .AnyAsync(x => x.LeagueId == request.LeagueId && x.UserId == userId, ct);

            if (!isMember)
                return Result<GetChatMessagesResponse>.Forbidden("you are not a member of this league", "user.forbidden");

            var take = Math.Clamp(request.Take, 1, 100);

            // 3) Query messages
            var query = _db.ChatMessages
                .AsNoTracking()
                .Where(x => x.LeagueId == request.LeagueId && !x.IsDeleted);

            if (request.Cursor is not null)
                query = query.Where(x => x.CreatedAt < request.Cursor.Value);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .Select(x => new ChatMessageDto
                {
                    Id = x.Id,
                    LeagueId = x.LeagueId,
                    UserId = x.UserId,
                    Username = x.User.Username,
                    AvatarUrl = x.User.AvatarUrl,
                    Message = x.Message,
                    CreatedAt = x.CreatedAt,
                    IsEdited = x.IsEdited,
                    EditedAt = x.EditedAt
                })
                .ToListAsync(ct);

            DateTime? nextCursor = items.Count == 0 ? (DateTime?)null : items[^1].CreatedAt;

            var response = new GetChatMessagesResponse
            {
                Items = items,
                NextCursor = nextCursor
            };

            return Result<GetChatMessagesResponse>.Success(response);
        }
    }
}
