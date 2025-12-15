using Domain.Common;

namespace Domain.Entities
{
    public class LeagueMember : BaseEntity
    {
        public Guid LeagueId { get; set; }
        public Guid UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsMuted { get; set; } = false;

        // Navigation properties
        public League League { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
