using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class BonusQuestion : BaseEntity
    {
        public Guid TournamentId { get; set; }

        public BonusQuestionType QuestionType { get; set; }
        public string Question { get; set; } = string.Empty;
        
        public Guid? AnswerTeamId { get; set; }
        public Guid? AnswerPlayerId { get; set; }  // For player-based questions (e.g., top scorer)
        public string? AnswerText { get; set; }

        public bool IsResolved { get; set; } = false;
        public int Points { get; set; }

        // Navigation properties
        public Tournament Tournament { get; set; } = null!;
        public Team? AnswerTeam { get; set; }
        public Player? AnswerPlayer { get; set; }
        public ICollection<BonusPrediction> Predictions { get; set; } = new List<BonusPrediction>();
    }
}
