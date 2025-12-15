using Domain.Common;

namespace Domain.Entities
{
    public class ChatMessage : BaseEntity
    {
        public Guid LeagueId { get; set; }
        public Guid UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public League League { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
