using API.Contracts.Predictions;
using Application.Common;
using Application.Features.Predictions.Commands.BulkSubmitPredictions;
using Application.Features.Predictions.Commands.SubmitPrediction;
using Application.Features.Predictions.Commands.UpdatePrediction;
using Application.Features.Predictions.DTOs;
using Application.Features.Predictions.Queries.GetPrediction;
using Application.Features.Predictions.Queries.GetUserPredictions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/predictions")]
    [Authorize]
    public class PredictionsController : BaseApiController
    {
        private readonly ISender _mediator;
        public PredictionsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<Result<Guid>>> Submit([FromBody] SubmitPredictionRequest request, CancellationToken ct)
        {
            var command = new SubmitPredictionCommand(
                request.LeagueId,
                request.MatchId,
                request.HomeScore,
                request.AwayScore
            );

            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<Result<BulkSubmitPredictionsResult>>> BulkSubmit([FromBody] BulkSubmitPredictionsRequest request, CancellationToken ct)
        {
            var predictions = request.Predictions
                .Select(p => new PredictionItem(p.MatchId, p.HomeScore, p.AwayScore))
                .ToList();

            var command = new BulkSubmitPredictionsCommand(request.LeagueId, predictions);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Result<bool>>> Update([FromRoute] Guid id, [FromBody] UpdatePredictionRequest request, CancellationToken ct)
        {
            var command = new UpdatePredictionCommand(
                id,
                request.HomeScore,
                request.AwayScore
            );

            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpGet]
        public async Task<ActionResult<Result<List<PredictionDto>>>> GetMine([FromQuery] Guid leagueId, CancellationToken ct)
        {
            var query = new GetUserPredictionsQuery(leagueId);

            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        [HttpGet("match/{matchId:guid}")]
        public async Task<ActionResult<Result<PredictionDto?>>> GetForMatch([FromRoute] Guid matchId, [FromQuery] Guid leagueId, CancellationToken ct)
        {
            var query = new GetPredictionQuery(leagueId, matchId);

            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }
    }
}
