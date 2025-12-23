using Application.Common;
using Application.Features.Admin.DTOs;
using Application.Features.Admin.Predictions.Commands.DeletePrediction;
using Application.Features.Admin.Predictions.Commands.RecalculateLeaguePredictions;
using Application.Features.Admin.Predictions.Queries.GetAdminPredictionById;
using Application.Features.Admin.Predictions.Queries.GetAdminPredictions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/predictions")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminPredictionsController : BaseApiController
    {
        private readonly ISender _mediator;

        public AdminPredictionsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<Result<PagedResult<AdminPredictionListDto>>>> GetPredictions(
            [FromQuery] Guid? leagueId,
            [FromQuery] Guid? matchId,
            [FromQuery] Guid? userId,
            [FromQuery] Guid? tournamentId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var query = new GetAdminPredictionsQuery(leagueId, matchId, userId, tournamentId, page, pageSize);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        [HttpGet("{predictionId:guid}")]
        public async Task<ActionResult<Result<AdminPredictionDto>>> GetPredictionById(
            Guid predictionId,
            CancellationToken ct = default)
        {
            var query = new GetAdminPredictionByIdQuery(predictionId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        [HttpDelete("{predictionId:guid}")]
        public async Task<ActionResult<Result<bool>>> DeletePrediction(
            Guid predictionId,
            CancellationToken ct = default)
        {
            var command = new DeletePredictionCommand(predictionId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPost("recalculate")]
        public async Task<ActionResult<Result<RecalculateLeaguePredictionsResult>>> RecalculateLeaguePredictions(
            [FromQuery] Guid leagueId,
            CancellationToken ct = default)
        {
            var command = new RecalculateLeaguePredictionsCommand(leagueId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
