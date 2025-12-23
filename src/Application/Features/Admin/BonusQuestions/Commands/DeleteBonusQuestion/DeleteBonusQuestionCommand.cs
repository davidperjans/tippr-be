using Application.Common;
using MediatR;

namespace Application.Features.Admin.BonusQuestions.Commands.DeleteBonusQuestion
{
    public sealed record DeleteBonusQuestionCommand(Guid BonusQuestionId) : IRequest<Result<bool>>;
}
