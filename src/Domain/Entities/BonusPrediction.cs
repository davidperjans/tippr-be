using Domain.Common;

namespace Domain.Entities
{
    public class BonusPrediction : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid BonusQuestionId { get; set; }
        public Guid LeagueId { get; set; }

        public Guid? AnswerTeamId { get; set; }
        public Guid? AnswerPlayerId { get; set; }  // For player-based predictions (e.g., top scorer)
        public string? AnswerText { get; set; }

        public int? PointsEarned { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public BonusQuestion BonusQuestion { get; set; } = null!;
        public League League { get; set; } = null!;
        public Team? AnswerTeam { get; set; }
        public Player? AnswerPlayer { get; set; }
    }
}
