namespace API.Contracts.Leagues
{
    public sealed record UpdateLeagueSettingsRequest(
        string PredictionMode,          // ex: "AllAtOnce", "StageByStage", "MatchByMatch"
        int DeadlineMinutes,
        int PointsCorrectScore,
        int PointsCorrectOutcome,
        int PointsCorrectGoals,
        int PointsRoundOf16Team,
        int PointsQuarterFinalTeam,
        int PointsSemiFinalTeam,
        int PointsFinalTeam,
        int PointsTopScorer,
        int PointsWinner,
        int PointsMostGoalsGroup,
        int PointsMostConcededGroup,
        bool AllowLateEdits
    );
}
