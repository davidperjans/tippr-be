namespace Application.Features.Predictions.DTOs
{
    public sealed class PredictionDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public Guid LeagueId { get; init; }
        public Guid MatchId { get; init; }
        public int HomeScore { get; init; }
        public int AwayScore { get; init; }
        public int? PointsEarned { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
}
