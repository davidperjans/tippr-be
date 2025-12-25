using Domain.Common;

namespace Domain.Entities
{
    public class Venue : BaseEntity
    {
        public int? ApiFootballId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public int? Capacity { get; set; }
        public string? Surface { get; set; }
        public string? ImageUrl { get; set; }

        public ICollection<Team> Teams { get; set; } = new List<Team>();
        public ICollection<Match> Matches { get; set; } = new List<Match>();
    }
}
