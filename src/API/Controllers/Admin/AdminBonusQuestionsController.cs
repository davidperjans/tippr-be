using API.Contracts.Admin;
using Application.Common;
using Application.Features.Admin.BonusPredictions.Queries.GetAdminBonusPredictions;
using Application.Features.Admin.BonusQuestions.Commands.DeleteBonusQuestion;
using Application.Features.Admin.BonusQuestions.Commands.RecalculateBonusQuestion;
using Application.Features.Admin.BonusQuestions.Commands.UpdateBonusQuestion;
using Application.Features.Admin.BonusQuestions.Queries.GetAdminBonusQuestionById;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/bonus-questions")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminBonusQuestionsController : BaseApiController
    {
        private readonly ISender _mediator;

        public AdminBonusQuestionsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{bonusQuestionId:guid}")]
        public async Task<ActionResult<Result<AdminBonusQuestionDto>>> GetBonusQuestionById(
            Guid bonusQuestionId,
            CancellationToken ct = default)
        {
            var query = new GetAdminBonusQuestionByIdQuery(bonusQuestionId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        [HttpPut("{bonusQuestionId:guid}")]
        public async Task<ActionResult<Result<AdminBonusQuestionDto>>> UpdateBonusQuestion(
            Guid bonusQuestionId,
            [FromBody] UpdateBonusQuestionRequest request,
            CancellationToken ct = default)
        {
            var command = new UpdateBonusQuestionCommand(
                bonusQuestionId,
                request.QuestionType,
                request.Question,
                request.Points
            );
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpDelete("{bonusQuestionId:guid}")]
        public async Task<ActionResult<Result<bool>>> DeleteBonusQuestion(
            Guid bonusQuestionId,
            CancellationToken ct = default)
        {
            var command = new DeleteBonusQuestionCommand(bonusQuestionId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPost("{bonusQuestionId:guid}/recalculate")]
        public async Task<ActionResult<Result<RecalculateBonusQuestionResult>>> RecalculateBonusQuestion(
            Guid bonusQuestionId,
            CancellationToken ct = default)
        {
            var command = new RecalculateBonusQuestionCommand(bonusQuestionId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }

    [ApiController]
    [Route("api/admin/bonus-predictions")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminBonusPredictionsController : BaseApiController
    {
        private readonly ISender _mediator;

        public AdminBonusPredictionsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<Result<PagedResult<AdminBonusPredictionListDto>>>> GetBonusPredictions(
            [FromQuery] Guid? leagueId,
            [FromQuery] Guid? questionId,
            [FromQuery] Guid? userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var query = new GetAdminBonusPredictionsQuery(leagueId, questionId, userId, page, pageSize);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }
    }
}
