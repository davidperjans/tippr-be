using API.Contracts.BonusQuestions;
using Application.Common;
using Application.Features.BonusQuestions.Commands.SubmitBonusPrediction;
using Application.Features.BonusQuestions.Queries.GetUserBonusPredictions;
using Application.Features.Predictions.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/bonus-predictions")]
    [Authorize]
    public class BonusPredictionController : BaseApiController
    {
        private readonly ISender _mediator;

        public BonusPredictionController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Submit a prediction for a bonus question
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Result<Guid>>> Submit(
            [FromBody] SubmitBonusPredictionRequest request,
            CancellationToken ct)
        {
            var command = new SubmitBonusPredictionCommand(
                request.LeagueId,
                request.BonusQuestionId,
                request.AnswerTeamId,
                request.AnswerText
            );

            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Get current user's bonus predictions for a league
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Result<List<BonusPredictionDto>>>> GetMine(
            [FromQuery] Guid leagueId,
            CancellationToken ct)
        {
            var query = new GetUserBonusPredictionsQuery(leagueId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }
    }
}
