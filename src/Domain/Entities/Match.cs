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
        public string? Venue { get; set; }
        public int? ApiFootballId { get; set; }

        // Navigation properties
        public Tournament Tournament { get; set; } = null!;
        public Team HomeTeam { get; set; } = null!;
        public Team AwayTeam { get; set; } = null!;
        public ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();
    }
}
