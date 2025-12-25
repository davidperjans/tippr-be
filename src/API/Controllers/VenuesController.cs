using Application.Common;
using Application.Features.Venues.DTOs;
using Application.Features.Venues.Queries.GetVenue;
using Application.Features.Venues.Queries.GetVenues;
using Application.Features.Venues.Queries.GetVenueByMatch;
using Application.Features.Venues.Queries.GetVenueByTeam;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/venues")]
    [Authorize]
    public sealed class VenuesController : BaseApiController
    {
        private readonly ISender _mediator;

        public VenuesController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all venues, optionally filtered by tournament
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Result<IReadOnlyList<VenueDto>>>> GetVenues(
            [FromQuery] Guid? tournamentId,
            CancellationToken ct)
        {
            var query = new GetVenuesQuery(tournamentId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Get venue by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Result<VenueDto>>> GetVenue(Guid id, CancellationToken ct)
        {
            var query = new GetVenueQuery(id);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Get venue by team ID (team's home venue)
        /// </summary>
        [HttpGet("by-team/{teamId:guid}")]
        public async Task<ActionResult<Result<VenueDto>>> GetVenueByTeam(Guid teamId, CancellationToken ct)
        {
            var query = new GetVenueByTeamQuery(teamId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Get venue by match ID
        /// </summary>
        [HttpGet("by-match/{matchId:guid}")]
        public async Task<ActionResult<Result<VenueDto>>> GetVenueByMatch(Guid matchId, CancellationToken ct)
        {
            var query = new GetVenueByMatchQuery(matchId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }
    }
}
