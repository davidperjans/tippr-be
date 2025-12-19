using API.Contracts.Matches;
using Application.Common;
using Application.Features.Matches.Commands.UpdateMatchResult;
using Application.Features.Matches.DTOs;
using Application.Features.Matches.Queries.GetMatch;
using Application.Features.Matches.Queries.GetMatchesByDate;
using Application.Features.Matches.Queries.GetMatchesByTournament;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/matches")]
    [Authorize]
    public class MatchesController : BaseApiController
    {
        private readonly ISender _mediator;
        public MatchesController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<Result<IReadOnlyList<MatchListItemDto>>>> Get([FromQuery] Guid? tournamentId, [FromQuery] DateOnly? date, CancellationToken ct)
        {
            if (tournamentId.HasValue)
                return FromResult(await _mediator.Send(new GetMatchesByTournamentQuery(tournamentId.Value)));

            if (date.HasValue)
                return FromResult(await _mediator.Send(new GetMatchesByDateQuery(date.Value)));

            return BadRequest("Provide either tournamentId or date.");
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Result<MatchDetailDto>>> GetById(Guid id, CancellationToken ct)
        {
            var query = new GetMatchQuery(id);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        [HttpPut("{id:guid}/result")]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult<Result<bool>>> UpdateResult(Guid id, [FromBody] UpdateMatchResultRequest body, CancellationToken ct)
        {
            var command = new UpdateMatchResultCommand(id, body.HomeScore, body.AwayScore, body.Status);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
