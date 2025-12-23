using Domain.Common;

namespace Domain.Entities
{
    public class Team : BaseEntity
    {
        public Guid TournamentId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? DisplayName { get; set; }  // Localized name (e.g., Swedish) for frontend display
        public string? Code { get; set; }
        public string? LogoUrl { get; set; }
        public Guid? GroupId { get; set; }  // FK to Group entity
        public int? FoundedYear { get; set; }

        // FIFA enrichment
        public int? FifaRank { get; set; }
        public decimal? FifaPoints { get; set; }
        public DateTime? FifaRankingUpdatedAt { get; set; }
        
        // API-FOOTBALL
        public int? ApiFootballId { get; set; }

        // Home venue
        public Guid? VenueId { get; set; }


        // Navigation properties
        public Tournament Tournament { get; set; } = null!;
        public Group? Group { get; set; }
        public Venue? Venue { get; set; }

        public ICollection<Match> HomeMatches { get; set; } = new List<Match>();
        public ICollection<Match> AwayMatches { get; set; } = new List<Match>();

        public ICollection<User> FavoriteByUsers { get; set; } = new List<User>();

        public ICollection<Player> Players { get; set; } = new List<Player>();

        public ICollection<BonusQuestion> BonusQuestionsAnswered { get; set; } = new List<BonusQuestion>();
        public ICollection<BonusPrediction> BonusPredictions { get; set; } = new List<BonusPrediction>();
    }
}
