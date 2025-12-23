using Domain.Enums;

namespace Application.Features.Admin.DTOs
{
    public class AdminBonusQuestionDto
    {
        public Guid Id { get; init; }
        public Guid TournamentId { get; init; }
        public string TournamentName { get; init; } = string.Empty;
        public BonusQuestionType QuestionType { get; init; }
        public string Question { get; init; } = string.Empty;
        public Guid? AnswerTeamId { get; init; }
        public string? AnswerTeamName { get; init; }
        public string? AnswerText { get; init; }
        public bool IsResolved { get; init; }
        public int Points { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public int PredictionCount { get; init; }
    }

    public class AdminBonusPredictionDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Username { get; init; } = string.Empty;
        public string UserDisplayName { get; init; } = string.Empty;
        public Guid BonusQuestionId { get; init; }
        public string QuestionText { get; init; } = string.Empty;
        public Guid LeagueId { get; init; }
        public string LeagueName { get; init; } = string.Empty;
        public Guid? AnswerTeamId { get; init; }
        public string? AnswerTeamName { get; init; }
        public string? AnswerText { get; init; }
        public int? PointsEarned { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }

    public class AdminBonusPredictionListDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Username { get; init; } = string.Empty;
        public Guid BonusQuestionId { get; init; }
        public Guid LeagueId { get; init; }
        public string LeagueName { get; init; } = string.Empty;
        public Guid? AnswerTeamId { get; init; }
        public string? AnswerTeamName { get; init; }
        public string? AnswerText { get; init; }
        public int? PointsEarned { get; init; }
    }
}
