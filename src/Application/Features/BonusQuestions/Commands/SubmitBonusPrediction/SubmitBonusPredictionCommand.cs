using Application.Common;
using MediatR;

namespace Application.Features.BonusQuestions.Commands.SubmitBonusPrediction
{
    public sealed record SubmitBonusPredictionCommand(
        Guid LeagueId,
        Guid BonusQuestionId,
        Guid? AnswerTeamId,
        string? AnswerText
    ) : IRequest<Result<Guid>>;
}
