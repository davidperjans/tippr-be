using Domain.Enums;

namespace Application.Features.Predictions.DTOs
{
    public sealed class BonusQuestionDto
    {
        public Guid Id { get; init; }
        public Guid TournamentId { get; init; }
        public BonusQuestionType QuestionType { get; init; }
        public string Question { get; init; } = string.Empty;
        public int Points { get; init; }
        public bool IsResolved { get; init; }
        public Guid AnswerTeamId { get; init; }
        public string AnswerText { get; init; } = string.Empty;
    }
}
