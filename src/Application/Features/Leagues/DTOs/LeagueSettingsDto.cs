namespace Application.Features.Leagues.DTOs
{
    public sealed class LeagueSettingsDto
    {
        public Guid LeagueId { get; set; }

        public string PredictionMode { get; set; } = string.Empty;
        public int DeadlineMinutes { get; set; }

        public int PointsCorrectScore { get; set; }
        public int PointsCorrectOutcome { get; set; }
        public int PointsCorrectGoals { get; set; }

        public int PointsRoundOf16Team { get; set; }
        public int PointsQuarterFinalTeam { get; set; }
        public int PointsSemiFinalTeam { get; set; }
        public int PointsFinalTeam { get; set; }

        public int PointsTopScorer { get; set; }
        public int PointsWinner { get; set; }
        public int PointsMostGoalsGroup { get; set; }
        public int PointsMostConcededGroup { get; set; }

        public bool AllowLateEdits { get; set; }
    }
}
