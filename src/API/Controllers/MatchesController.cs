using API.Contracts.Errors;
using API.Contracts.Matches;
using Application.Common;
using Application.Features.Matches.Commands.UpdateMatchResult;
using Application.Features.Matches.DTOs;
using Application.Features.Matches.Queries.GetMatch;
using Application.Features.Matches.Queries.GetMatchesByDate;
using Application.Features.Matches.Queries.GetMatchesByTeam;
using Application.Features.Matches.Queries.GetMatchesByTournament;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Provides access to match data and match result management.
    /// </summary>
    /// <remarks>
    /// <para><b>Domain Relationships:</b></para>
    /// <list type="bullet">
    ///   <item><description>Matches belong to a Tournament</description></item>
    ///   <item><description>Matches have a HomeTeam and AwayTeam</description></item>
    ///   <item><description>Matches are played at a Venue</description></item>
    ///   <item><description>Users submit Predictions for Matches within a League context</description></item>
    /// </list>
    ///
    /// <para><b>Match Statuses:</b></para>
    /// <list type="bullet">
    ///   <item><description>Scheduled: Match not yet started</description></item>
    ///   <item><description>Live: Match in progress</description></item>
    ///   <item><description>Finished: Match completed with final score</description></item>
    ///   <item><description>Postponed: Match delayed to later date</description></item>
    ///   <item><description>Cancelled: Match will not be played</description></item>
    /// </list>
    /// </remarks>
    [ApiController]
    [Route("api/matches")]
    [Authorize]
    [Produces("application/json")]
    public class MatchesController : BaseApiController
    {
        private readonly ISender _mediator;

        public MatchesController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves matches filtered by tournament ID or date.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Filter:</b> Either tournamentId OR date must be provided (not both, not neither)</para>
        ///
        /// <para><b>Example Request (by tournament):</b></para>
        /// <code>
        /// GET /api/matches?tournamentId=3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Request (by date):</b></para>
        /// <code>
        /// GET /api/matches?date=2026-06-15
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": [
        ///     {
        ///       "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "homeTeam": { "id": "...", "name": "Brazil", "shortName": "BRA" },
        ///       "awayTeam": { "id": "...", "name": "Germany", "shortName": "GER" },
        ///       "kickoffTime": "2026-06-15T18:00:00Z",
        ///       "status": "Scheduled",
        ///       "homeScore": null,
        ///       "awayScore": null,
        ///       "round": "Group A"
        ///     }
        ///   ],
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="tournamentId">Filter by tournament ID</param>
        /// <param name="date">Filter by match date (format: yyyy-MM-dd)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of matches matching the filter</returns>
        /// <response code="200">Returns the list of matches</response>
        /// <response code="400">Neither tournamentId nor date was provided</response>
        /// <response code="401">JWT token is missing or invalid</response>
        [HttpGet]
        [ProducesResponseType(typeof(Result<IReadOnlyList<MatchListItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<IReadOnlyList<MatchListItemDto>>>> Get(
            [FromQuery] Guid? tournamentId,
            [FromQuery] DateOnly? date,
            CancellationToken ct)
        {
            if (tournamentId.HasValue)
                return FromResult(await _mediator.Send(new GetMatchesByTournamentQuery(tournamentId.Value)));

            if (date.HasValue)
                return FromResult(await _mediator.Send(new GetMatchesByDateQuery(date.Value)));

            return BadRequest("Provide either tournamentId or date.");
        }

        /// <summary>
        /// Retrieves detailed information for a specific match.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/matches/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "tournamentId": "...",
        ///     "homeTeam": { "id": "...", "name": "Brazil", "shortName": "BRA", "logoUrl": "..." },
        ///     "awayTeam": { "id": "...", "name": "Germany", "shortName": "GER", "logoUrl": "..." },
        ///     "venue": { "id": "...", "name": "Maracana", "city": "Rio de Janeiro" },
        ///     "kickoffTime": "2026-06-15T18:00:00Z",
        ///     "status": "Finished",
        ///     "homeScore": 2,
        ///     "awayScore": 1,
        ///     "round": "Final"
        ///   },
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">The unique identifier of the match</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The match details including venue and full team info</returns>
        /// <response code="200">Returns the match details</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="404">Match not found</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Result<MatchDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<MatchDetailDto>>> GetById(Guid id, CancellationToken ct)
        {
            var query = new GetMatchQuery(id);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Retrieves all matches for a specific team (home and away).
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/matches/by-team/3fa85f64-5717-4562-b3fc-2c963f66afa6
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
        ///       "homeTeam": { "id": "...", "name": "Brazil" },
        ///       "awayTeam": { "id": "...", "name": "Germany" },
        ///       "kickoffTime": "2026-06-15T18:00:00Z",
        ///       "status": "Finished",
        ///       "homeScore": 2,
        ///       "awayScore": 1
        ///     }
        ///   ],
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="teamId">The team ID to get matches for</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of all matches where the team plays (home or away)</returns>
        /// <response code="200">Returns the list of matches</response>
        /// <response code="401">JWT token is missing or invalid</response>
        [HttpGet("by-team/{teamId:guid}")]
        [ProducesResponseType(typeof(Result<IReadOnlyList<MatchListItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<IReadOnlyList<MatchListItemDto>>>> GetByTeam(
            Guid teamId,
            CancellationToken ct)
        {
            var query = new GetMatchesByTeamQuery(teamId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Updates the result (score and status) of a match (Admin only).
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required (Admin role)</para>
        /// <para><b>Side Effects:</b></para>
        /// <list type="bullet">
        ///   <item><description>Triggers scoring of predictions when status becomes "Finished"</description></item>
        ///   <item><description>Updates league standings for all leagues with predictions on this match</description></item>
        /// </list>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// PUT /api/matches/3fa85f64-5717-4562-b3fc-2c963f66afa6/result
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: application/json
        ///
        /// {
        ///   "homeScore": 2,
        ///   "awayScore": 1,
        ///   "status": "Finished"
        /// }
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": true,
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">The match ID to update</param>
        /// <param name="body">The new score and status</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if update was successful</returns>
        /// <response code="200">Match result updated successfully</response>
        /// <response code="400">Invalid score or status value</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User does not have Admin role</response>
        /// <response code="404">Match not found</response>
        [HttpPut("{id:guid}/result")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<bool>>> UpdateResult(
            Guid id,
            [FromBody] UpdateMatchResultRequest body,
            CancellationToken ct)
        {
            var command = new UpdateMatchResultCommand(id, body.HomeScore, body.AwayScore, body.Status);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
