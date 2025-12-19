using Domain.Enums;

namespace API.Contracts.BonusQuestions
{
    public sealed record CreateBonusQuestionRequest(
        Guid TournamentId,
        BonusQuestionType QuestionType,
        string Question,
        int Points
    );
}
