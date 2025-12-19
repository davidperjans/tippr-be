using Application.Common;
using MediatR;

namespace Application.Features.BonusQuestions.Commands.ResolveBonusQuestion
{
    public sealed record ResolveBonusQuestionCommand(
        Guid BonusQuestionId,
        Guid? AnswerTeamId,
        string? AnswerText
    ) : IRequest<Result<int>>;  // Returns number of predictions awarded points
}
