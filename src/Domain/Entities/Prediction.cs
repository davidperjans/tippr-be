using Domain.Common;

namespace Domain.Entities
{
    public class Prediction : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid MatchId { get; set; }
        public Guid LeagueId { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public int? PointsEarned { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Match Match { get; set; } = null!;
        public League League { get; set; } = null!;
    }
}
