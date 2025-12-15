using Domain.Common;

namespace Domain.Entities
{
    public class LeagueStanding : BaseEntity
    {
        public Guid LeagueId { get; set; }
        public Guid UserId { get; set; }
        public int TotalPoints { get; set; } = 0;
        public int MatchPoints { get; set; } = 0;
        public int BonusPoints { get; set; } = 0;
        public int Rank { get; set; }
        public int? PreviousRank { get; set; }

        // Navigation properties
        public League League { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
