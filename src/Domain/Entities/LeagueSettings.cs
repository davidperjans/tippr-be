using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class LeagueSettings : BaseEntity
    {
        public Guid LeagueId { get; set; }

        public PredictionMode PredictionMode { get; set; }
        public int DeadlineMinutes { get; set; } = 60;
        
        public int PointsCorrectScore { get; set; } = 7;
        public int PointsCorrectOutcome { get; set; } = 3;
        public int PointsCorrectGoals { get; set; } = 2;
        
        public int PointsRoundOf16Team { get; set; } = 2;
        public int PointsQuarterFinalTeam { get; set; } = 4;
        public int PointsSemiFinalTeam { get; set; } = 6;
        public int PointsFinalTeam { get; set; } = 8;
        
        public int PointsTopScorer { get; set; } = 20;
        public int PointsWinner { get; set; } = 20;
        
        public int PointsMostGoalsGroup { get; set; } = 10;
        public int PointsMostConcededGroup { get; set; } = 10;
        
        public bool AllowLateEdits { get; set; } = false;

        // Navigation properties
        public League League { get; set; } = null!;
    }
}
