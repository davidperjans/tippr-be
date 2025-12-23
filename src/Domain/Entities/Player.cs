using Domain.Common;

namespace Domain.Entities
{
    public class Player : BaseEntity
    {
        public Guid TeamId { get; set; }

        public string Name { get; set; } = string.Empty;
        public int? Number { get; set; }  // Jersey number
        public string? Position { get; set; }  // Goalkeeper, Defender, Midfielder, Attacker
        public string? PhotoUrl { get; set; }

        // Player details
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public string? Nationality { get; set; }
        public int? Height { get; set; }  // in cm
        public int? Weight { get; set; }  // in kg
        public bool? Injured { get; set; }

        // API-FOOTBALL
        public int? ApiFootballId { get; set; }

        // Navigation properties
        public Team Team { get; set; } = null!;

        // For top scorer predictions
        public ICollection<BonusQuestion> BonusQuestionsAnswered { get; set; } = new List<BonusQuestion>();
        public ICollection<BonusPrediction> BonusPredictions { get; set; } = new List<BonusPrediction>();
    }
}
