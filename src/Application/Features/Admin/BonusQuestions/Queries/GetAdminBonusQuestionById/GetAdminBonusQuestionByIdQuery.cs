using Application.Common;
using Application.Features.Admin.DTOs;
using MediatR;

namespace Application.Features.Admin.BonusQuestions.Queries.GetAdminBonusQuestionById
{
    public sealed record GetAdminBonusQuestionByIdQuery(Guid BonusQuestionId) : IRequest<Result<AdminBonusQuestionDto>>;
}
