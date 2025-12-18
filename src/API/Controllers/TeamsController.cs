using Application.Common;
using Application.Features.Teams.DTOs;
using Application.Features.Teams.Queries.GetTeam;
using Application.Features.Teams.Queries.GetTeamsByTournament;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/teams")]
    [Authorize]
    public sealed class TeamsController : BaseApiController
    {
        private readonly ISender _mediator;
        public TeamsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<Result<IReadOnlyList<TeamDto>>>> GetTeams([FromQuery] Guid tournamentId, CancellationToken ct)
        {
            var query = new GetTeamsByTournamentQuery(tournamentId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Result<TeamDto>>> GetTeam([FromRoute] Guid id, CancellationToken ct)
        {
            var query = new GetTeamQuery(id);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }
    }
}
