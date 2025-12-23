using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Match : BaseEntity
    {
        public Guid TournamentId { get; set; }

        public Guid HomeTeamId { get; set; }
        public Guid AwayTeamId { get; set; }
        
        public DateTime MatchDate { get; set; }
        public MatchStage Stage { get; set; }
        
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        
        public MatchStatus Status { get; set; }
        
        public Guid? VenueId { get; set; }
        public string? VenueName { get; set; }
        public string? VenueCity { get; set; }

        // API-FOOTBALL
        public int? ApiFootballId { get; set; }


        public int ResultVersion { get; set; }

        

        // Navigation properties
        public Tournament Tournament { get; set; } = null!;
        public Team HomeTeam { get; set; } = null!;
        public Team AwayTeam { get; set; } = null!;
        public Venue? Venue { get; set; }

        public ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();
    }
}
