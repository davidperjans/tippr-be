namespace API.Contracts.BonusQuestions
{
    public sealed record ResolveBonusQuestionRequest(
        Guid? AnswerTeamId,
        string? AnswerText
    );
}
