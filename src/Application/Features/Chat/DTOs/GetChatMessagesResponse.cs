namespace Application.Features.Chat.DTOs
{

    public sealed class GetChatMessagesResponse
    {
        public List<ChatMessageDto> Items { get; set; } = [];
        public DateTime? NextCursor { get; set; } // CreatedAt för sista item
    }
}
