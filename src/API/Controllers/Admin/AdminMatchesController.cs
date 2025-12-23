using API.Contracts.Admin;
using API.Contracts.Matches;
using Application.Common;
using Application.Features.Admin.DTOs;
using Application.Features.Admin.Matches.Commands.BulkCreateMatches;
using Application.Features.Admin.Matches.Commands.CreateMatch;
using Application.Features.Admin.Matches.Commands.RecalculateMatchPoints;
using Application.Features.Admin.Matches.Commands.UpdateMatch;
using Application.Features.Matches.Commands.UpdateMatchResult;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/matches")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminMatchesController : BaseApiController
    {
        private readonly ISender _mediator;

        public AdminMatchesController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<Result<Guid>>> CreateMatch(
            [FromBody] CreateMatchRequest request,
            CancellationToken ct = default)
        {
            var command = new CreateMatchCommand(
                request.TournamentId,
                request.HomeTeamId,
                request.AwayTeamId,
                request.MatchDate,
                request.Stage,
                request.Venue,
                request.ApiFootballId
            );
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPut("{matchId:guid}")]
        public async Task<ActionResult<Result<AdminMatchDto>>> UpdateMatch(
            Guid matchId,
            [FromBody] UpdateMatchRequest request,
            CancellationToken ct = default)
        {
            var command = new UpdateMatchCommand(
                matchId,
                request.MatchDate,
                request.Stage,
                request.Status,
                request.Venue,
                request.ApiFootballId
            );
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPut("{matchId:guid}/result")]
        public async Task<ActionResult<Result<bool>>> UpdateMatchResult(
            Guid matchId,
            [FromBody] UpdateMatchResultRequest request,
            CancellationToken ct = default)
        {
            var command = new UpdateMatchResultCommand(matchId, request.HomeScore, request.AwayScore, request.Status);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<Result<BulkCreateMatchesResult>>> BulkCreateMatches(
            [FromBody] BulkCreateMatchesRequest request,
            CancellationToken ct = default)
        {
            var matches = request.Matches.Select(m => new BulkMatchItem(
                m.HomeTeamId,
                m.AwayTeamId,
                m.MatchDate,
                m.Stage,
                m.Venue,
                m.ApiFootballId
            )).ToList();

            var command = new BulkCreateMatchesCommand(request.TournamentId, matches);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPost("{matchId:guid}/recalculate")]
        public async Task<ActionResult<Result<RecalculateMatchPointsResult>>> RecalculateMatchPoints(
            Guid matchId,
            CancellationToken ct = default)
        {
            var command = new RecalculateMatchPointsCommand(matchId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
