namespace API.Contracts.Predictions
{
    public sealed record SubmitPredictionRequest(
        Guid LeagueId,
        Guid MatchId,
        int HomeScore,
        int AwayScore
    );

    public sealed record BulkPredictionItem(
        Guid MatchId,
        int HomeScore,
        int AwayScore
    );

    public sealed record BulkSubmitPredictionsRequest(
        Guid LeagueId,
        List<BulkPredictionItem> Predictions
    );
}
