using Domain.Common;

namespace Domain.Entities
{
    public class GroupStanding : BaseEntity
    {
        public Guid GroupId { get; set; }
        public Guid TeamId { get; set; }

        public int Position { get; set; }  // 1, 2, 3, 4
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference { get; set; }
        public int Points { get; set; }

        // Optional: form/recent results (e.g., "WWDLW")
        public string? Form { get; set; }

        // Navigation properties
        public Group Group { get; set; } = null!;
        public Team Team { get; set; } = null!;
    }
}
