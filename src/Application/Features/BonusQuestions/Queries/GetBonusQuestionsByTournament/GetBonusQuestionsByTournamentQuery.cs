using Application.Common;
using Application.Features.Predictions.DTOs;
using MediatR;

namespace Application.Features.BonusQuestions.Queries.GetBonusQuestionsByTournament
{
    public sealed record GetBonusQuestionsByTournamentQuery(
        Guid TournamentId
    ) : IRequest<Result<IReadOnlyList<BonusQuestionDto>>>;
}
