using API.Contracts.Errors;
using Application.Common;
using Application.Features.Venues.DTOs;
using Application.Features.Venues.Queries.GetVenue;
using Application.Features.Venues.Queries.GetVenueByMatch;
using Application.Features.Venues.Queries.GetVenueByTeam;
using Application.Features.Venues.Queries.GetVenues;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Provides read-only access to venue (stadium) data.
    /// </summary>
    /// <remarks>
    /// <para><b>Domain Relationships:</b></para>
    /// <list type="bullet">
    ///   <item><description>Venues host Matches</description></item>
    ///   <item><description>Venues can be a Team's home ground</description></item>
    ///   <item><description>Venues are associated with Tournaments through their matches</description></item>
    /// </list>
    /// </remarks>
    [ApiController]
    [Route("api/venues")]
    [Authorize]
    [Produces("application/json")]
    public sealed class VenuesController : BaseApiController
    {
        private readonly ISender _mediator;

        public VenuesController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all venues, optionally filtered by tournament.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/venues?tournamentId=3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": [
        ///     {
        ///       "id": "...",
        ///       "name": "Maracana",
        ///       "city": "Rio de Janeiro",
        ///       "country": "Brazil",
        ///       "capacity": 78838,
        ///       "imageUrl": "https://..."
        ///     },
        ///     {
        ///       "id": "...",
        ///       "name": "Wembley Stadium",
        ///       "city": "London",
        ///       "country": "England",
        ///       "capacity": 90000,
        ///       "imageUrl": "https://..."
        ///     }
        ///   ],
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="tournamentId">Optional tournament ID to filter venues by</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of venues</returns>
        /// <response code="200">Returns the list of venues</response>
        /// <response code="401">JWT token is missing or invalid</response>
        [HttpGet]
        [ProducesResponseType(typeof(Result<IReadOnlyList<VenueDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<IReadOnlyList<VenueDto>>>> GetVenues(
            [FromQuery] Guid? tournamentId,
            CancellationToken ct)
        {
            var query = new GetVenuesQuery(tournamentId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Retrieves a specific venue by its unique identifier.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/venues/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "name": "Maracana",
        ///     "city": "Rio de Janeiro",
        ///     "country": "Brazil",
        ///     "capacity": 78838,
        ///     "imageUrl": "https://..."
        ///   },
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">The unique identifier of the venue</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The venue details</returns>
        /// <response code="200">Returns the venue</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="404">Venue not found</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Result<VenueDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<VenueDto>>> GetVenue(
            Guid id,
            CancellationToken ct)
        {
            var query = new GetVenueQuery(id);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Retrieves the home venue for a specific team.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/venues/by-team/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "id": "...",
        ///     "name": "Maracana",
        ///     "city": "Rio de Janeiro",
        ///     "country": "Brazil",
        ///     "capacity": 78838,
        ///     "imageUrl": "https://..."
        ///   },
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="teamId">The team ID to get the home venue for</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The team's home venue</returns>
        /// <response code="200">Returns the venue</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="404">Team or venue not found</response>
        [HttpGet("by-team/{teamId:guid}")]
        [ProducesResponseType(typeof(Result<VenueDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<VenueDto>>> GetVenueByTeam(
            Guid teamId,
            CancellationToken ct)
        {
            var query = new GetVenueByTeamQuery(teamId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Retrieves the venue for a specific match.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/venues/by-match/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "id": "...",
        ///     "name": "Wembley Stadium",
        ///     "city": "London",
        ///     "country": "England",
        ///     "capacity": 90000,
        ///     "imageUrl": "https://..."
        ///   },
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="matchId">The match ID to get the venue for</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The venue where the match is played</returns>
        /// <response code="200">Returns the venue</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="404">Match or venue not found</response>
        [HttpGet("by-match/{matchId:guid}")]
        [ProducesResponseType(typeof(Result<VenueDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<VenueDto>>> GetVenueByMatch(
            Guid matchId,
            CancellationToken ct)
        {
            var query = new GetVenueByMatchQuery(matchId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }
    }
}
