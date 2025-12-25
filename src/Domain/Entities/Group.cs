using Domain.Common;

namespace Domain.Entities
{
    public class Group : BaseEntity
    {
        public Guid TournamentId { get; set; }

        public string Name { get; set; } = string.Empty;  // "A", "B", "C", etc.

        public int? ApiFootballGroupId { get; set; }  // For API-Football mapping if available

        // Navigation properties
        public Tournament Tournament { get; set; } = null!;
        public ICollection<Team> Teams { get; set; } = new List<Team>();
        public ICollection<GroupStanding> Standings { get; set; } = new List<GroupStanding>();
    }
}
