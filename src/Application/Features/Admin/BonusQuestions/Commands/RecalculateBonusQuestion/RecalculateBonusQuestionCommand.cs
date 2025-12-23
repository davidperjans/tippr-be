using Application.Common;
using MediatR;

namespace Application.Features.Admin.BonusQuestions.Commands.RecalculateBonusQuestion
{
    public sealed record RecalculateBonusQuestionCommand(Guid BonusQuestionId) : IRequest<Result<RecalculateBonusQuestionResult>>;

    public class RecalculateBonusQuestionResult
    {
        public int PredictionsUpdated { get; init; }
        public int CorrectPredictions { get; init; }
        public int TotalPointsAwarded { get; init; }
        public int LeaguesAffected { get; init; }
    }
}
