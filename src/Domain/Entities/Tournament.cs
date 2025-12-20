using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Tournament : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Year { get; set; }
        public TournamentType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Team> Teams { get; set; } = new List<Team>();
        public ICollection<Match> Matches { get; set; } = new List<Match>();
        public ICollection<BonusQuestion> BonusQuestions { get; set; } = new List<BonusQuestion>();
        public ICollection<League> Leagues { get; set; } = new List<League>();
        public ICollection<TournamentCountry> Countries { get; set; } = new List<TournamentCountry>();
    }
}
