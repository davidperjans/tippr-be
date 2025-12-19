namespace API.Contracts.BonusQuestions
{
    public sealed record SubmitBonusPredictionRequest(
        Guid LeagueId,
        Guid BonusQuestionId,
        Guid? AnswerTeamId,
        string? AnswerText
    );
}
