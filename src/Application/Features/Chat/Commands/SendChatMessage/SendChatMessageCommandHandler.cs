using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Chat.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Chat.Commands.SendChatMessage
{
    public sealed class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, Result<ChatMessageDto>>
    {
        private readonly ITipprDbContext _db;
        private readonly ICurrentUser _currentUser;

        public SendChatMessageCommandHandler(ITipprDbContext db, ICurrentUser currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<Result<ChatMessageDto>> Handle(SendChatMessageCommand request, CancellationToken ct)
        {
            var userId = _currentUser.UserId;

            var message = request.Message?.Trim() ?? string.Empty;

            if (message.Length == 0)
                return Result<ChatMessageDto>.Validation(
                    new Dictionary<string, string[]> { ["message"] = ["Message cannot be empty."] });

            if (message.Length > 1000)
                return Result<ChatMessageDto>.Validation(
                    new Dictionary<string, string[]> { ["message"] = ["Message is too long. Max 1000 characters."] });

            // 1) League exists?
            var league = await _db.Leagues
                .AsNoTracking()
                .Where(x => x.Id == request.LeagueId)
                .Select(x => new { x.Id, x.IsGlobal })
                .FirstOrDefaultAsync(ct);

            if (league is null)
                return Result<ChatMessageDto>.NotFound("League not found.");

            // 2) Global league => no chat
            if (league.IsGlobal)
                return Result<ChatMessageDto>.BusinessRule("Chat is not enabled for global leagues.", "CHAT.DISABLED_FOR_GLOBAL_LEAGUE");

            // 3) Membership
            var member = await _db.LeagueMembers
                .AsNoTracking()
                .Where(x => x.LeagueId == request.LeagueId && x.UserId == userId)
                .Select(x => new { x.UserId, x.IsMuted })
                .FirstOrDefaultAsync(ct);

            if (member is null)
                return Result<ChatMessageDto>.Forbidden("You are not a member of this league.", "USER.FORBIDDEN");

            if (member.IsMuted)
                return Result<ChatMessageDto>.Forbidden("You are muted in this league.", code: "LEAGUE.MEMBER_MUTED");

            // 4) Persist
            var entity = new Domain.Entities.ChatMessage
            {
                Id = Guid.NewGuid(),
                LeagueId = request.LeagueId,
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsEdited = false,
                IsDeleted = false
            };

            _db.ChatMessages.Add(entity);
            await _db.SaveChangesAsync(ct);

            // Get userinfo for username and avatarurl
            var userInfo = await _db.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => new
                {
                    x.Username,
                    x.AvatarUrl
                })
                .FirstOrDefaultAsync(ct);

            if (userInfo == null)
                return Result<ChatMessageDto>.NotFound("user not found", "USER.NOT_FOUND");

            var dto = new ChatMessageDto
            {
                Id = entity.Id,
                LeagueId = entity.LeagueId,
                UserId = entity.UserId,
                Username = userInfo.Username,
                AvatarUrl = userInfo.AvatarUrl,
                Message = entity.Message,
                CreatedAt = entity.CreatedAt,
                IsEdited = entity.IsEdited,
                EditedAt = entity.EditedAt
            };

            return Result<ChatMessageDto>.Success(dto);
        }
    }
}
