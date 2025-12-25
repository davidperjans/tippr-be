using Application.Common;
using Application.Features.Players.DTOs;
using Application.Features.Players.Queries.GetPlayer;
using Application.Features.Players.Queries.GetPlayersByTeam;
using Application.Features.Players.Queries.GetPlayersByTournament;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/players")]
    [Authorize]
    public sealed class PlayersController : BaseApiController
    {
        private readonly ISender _mediator;

        public PlayersController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all players for a tournament, with optional position filter and search
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Result<IReadOnlyList<PlayerWithTeamDto>>>> GetPlayers(
            [FromQuery] Guid tournamentId,
            [FromQuery] string? position,
            [FromQuery] string? search,
            CancellationToken ct)
        {
            var query = new GetPlayersByTournamentQuery(tournamentId, position, search);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Get player by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Result<PlayerWithTeamDto>>> GetPlayer(Guid id, CancellationToken ct)
        {
            var query = new GetPlayerQuery(id);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Get all players for a team
        /// </summary>
        [HttpGet("by-team/{teamId:guid}")]
        public async Task<ActionResult<Result<IReadOnlyList<PlayerDto>>>> GetPlayersByTeam(
            Guid teamId,
            CancellationToken ct)
        {
            var query = new GetPlayersByTeamQuery(teamId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }
    }
}
