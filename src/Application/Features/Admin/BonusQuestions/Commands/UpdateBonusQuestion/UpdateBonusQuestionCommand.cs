using Application.Common;
using Application.Features.Admin.DTOs;
using Domain.Enums;
using MediatR;

namespace Application.Features.Admin.BonusQuestions.Commands.UpdateBonusQuestion
{
    public sealed record UpdateBonusQuestionCommand(
        Guid BonusQuestionId,
        BonusQuestionType? QuestionType,
        string? Question,
        int? Points
    ) : IRequest<Result<AdminBonusQuestionDto>>;
}
