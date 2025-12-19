using Application.Common;
using Domain.Enums;
using MediatR;

namespace Application.Features.BonusQuestions.Commands.CreateBonusQuestion
{
    public sealed record CreateBonusQuestionCommand(
        Guid TournamentId,
        BonusQuestionType QuestionType,
        string Question,
        int Points
    ) : IRequest<Result<Guid>>;
}
