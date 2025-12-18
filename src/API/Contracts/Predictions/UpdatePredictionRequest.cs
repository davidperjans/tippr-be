namespace API.Contracts.Predictions
{
    public sealed record UpdatePredictionRequest(
        int HomeScore,
        int AwayScore
    );
}
