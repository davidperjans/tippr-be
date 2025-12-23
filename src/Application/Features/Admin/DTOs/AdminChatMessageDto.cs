namespace Application.Features.Admin.DTOs
{
    public class AdminChatMessageDto
    {
        public Guid Id { get; init; }
        public Guid LeagueId { get; init; }
        public string LeagueName { get; init; } = string.Empty;
        public Guid UserId { get; init; }
        public string Username { get; init; } = string.Empty;
        public string UserDisplayName { get; init; } = string.Empty;
        public string? UserAvatarUrl { get; init; }
        public string Message { get; init; } = string.Empty;
        public bool IsEdited { get; init; }
        public DateTime? EditedAt { get; init; }
        public bool IsDeleted { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public class AdminChatMessagesResponse
    {
        public IReadOnlyList<AdminChatMessageDto> Messages { get; init; } = new List<AdminChatMessageDto>();
        public DateTime? NextCursor { get; init; }
        public bool HasMore { get; init; }
    }
}
