namespace Application.Features.Chat.DTOs
{
    public sealed class ChatMessageDto
    {
        public Guid Id { get; set; }
        public Guid LeagueId { get; set; }
        public Guid UserId { get; set; }

        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
    }
}
