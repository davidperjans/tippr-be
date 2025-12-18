namespace API.Contracts.Predictions
{
    public sealed record SubmitPredictionRequest(
        Guid LeagueId,
        Guid MatchId,
        int HomeScore,
        int AwayScore
    );
}
