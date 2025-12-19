using API.Contracts.BonusQuestions;
using Application.Common;
using Application.Features.BonusQuestions.Commands.CreateBonusQuestion;
using Application.Features.BonusQuestions.Commands.ResolveBonusQuestion;
using Application.Features.BonusQuestions.Queries.GetBonusQuestionsByTournament;
using Application.Features.Predictions.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/bonus-questions")]
    [Authorize]
    public class BonusQuestionController : BaseApiController
    {
        private readonly ISender _mediator;

        public BonusQuestionController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all bonus questions for a tournament
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Result<IReadOnlyList<BonusQuestionDto>>>> GetByTournament(
            [FromQuery] Guid tournamentId,
            CancellationToken ct)
        {
            var query = new GetBonusQuestionsByTournamentQuery(tournamentId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Create a new bonus question (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<Result<Guid>>> Create(
            [FromBody] CreateBonusQuestionRequest request,
            CancellationToken ct)
        {
            var command = new CreateBonusQuestionCommand(
                request.TournamentId,
                request.QuestionType,
                request.Question,
                request.Points
            );

            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Resolve a bonus question and award points (Admin only)
        /// </summary>
        [HttpPut("{id:guid}/resolve")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<Result<int>>> Resolve(
            [FromRoute] Guid id,
            [FromBody] ResolveBonusQuestionRequest request,
            CancellationToken ct)
        {
            var command = new ResolveBonusQuestionCommand(
                id,
                request.AnswerTeamId,
                request.AnswerText
            );

            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
