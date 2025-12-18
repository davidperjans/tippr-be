namespace Application.Features.Predictions.DTOs
{
    public sealed class BonusPredictionDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public Guid LeagueId { get; init; }
        public Guid BonusQuestionId { get; init; }
        public Guid AnswerTeamId { get; init; }
        public string AnswerText { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}
