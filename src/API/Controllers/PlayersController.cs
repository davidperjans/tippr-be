using API.Contracts.Errors;
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
    /// <summary>
    /// Provides read-only access to player data for teams in tournaments.
    /// </summary>
    /// <remarks>
    /// <para><b>Domain Relationships:</b></para>
    /// <list type="bullet">
    ///   <item><description>Players belong to a Team</description></item>
    ///   <item><description>Players are associated with Tournaments through their Team</description></item>
    ///   <item><description>Players can be answers to bonus questions (e.g., "Top Scorer")</description></item>
    /// </list>
    ///
    /// <para><b>Player Positions:</b></para>
    /// <list type="bullet">
    ///   <item><description>Goalkeeper</description></item>
    ///   <item><description>Defender</description></item>
    ///   <item><description>Midfielder</description></item>
    ///   <item><description>Attacker</description></item>
    /// </list>
    /// </remarks>
    [ApiController]
    [Route("api/players")]
    [Authorize]
    [Produces("application/json")]
    public sealed class PlayersController : BaseApiController
    {
        private readonly ISender _mediator;

        public PlayersController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all players for a tournament with optional filtering.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/players?tournamentId=3fa85f64-5717-4562-b3fc-2c963f66afa6&amp;position=Attacker&amp;search=Mbappe
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
        ///       "name": "Kylian Mbappe",
        ///       "position": "Attacker",
        ///       "number": 10,
        ///       "photoUrl": "https://...",
        ///       "teamId": "...",
        ///       "teamName": "France",
        ///       "teamLogoUrl": "https://..."
        ///     }
        ///   ],
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="tournamentId">The tournament ID to get players for (required)</param>
        /// <param name="position">Optional filter by position (Goalkeeper, Defender, Midfielder, Attacker)</param>
        /// <param name="search">Optional search term to filter by player name</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of players matching the filters</returns>
        /// <response code="200">Returns the list of players</response>
        /// <response code="401">JWT token is missing or invalid</response>
        [HttpGet]
        [ProducesResponseType(typeof(Result<IReadOnlyList<PlayerWithTeamDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
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
        /// Retrieves a specific player by their unique identifier.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/players/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "name": "Kylian Mbappe",
        ///     "position": "Attacker",
        ///     "number": 10,
        ///     "photoUrl": "https://...",
        ///     "teamId": "...",
        ///     "teamName": "France",
        ///     "teamLogoUrl": "https://..."
        ///   },
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">The unique identifier of the player</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The player details including team information</returns>
        /// <response code="200">Returns the player</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="404">Player not found</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Result<PlayerWithTeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<PlayerWithTeamDto>>> GetPlayer(
            Guid id,
            CancellationToken ct)
        {
            var query = new GetPlayerQuery(id);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Retrieves all players for a specific team.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/players/by-team/3fa85f64-5717-4562-b3fc-2c963f66afa6
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
        ///       "name": "Kylian Mbappe",
        ///       "position": "Attacker",
        ///       "number": 10,
        ///       "photoUrl": "https://..."
        ///     },
        ///     {
        ///       "id": "...",
        ///       "name": "Antoine Griezmann",
        ///       "position": "Attacker",
        ///       "number": 7,
        ///       "photoUrl": "https://..."
        ///     }
        ///   ],
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="teamId">The team ID to get players for</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of players in the team's squad</returns>
        /// <response code="200">Returns the list of players</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="404">Team not found</response>
        [HttpGet("by-team/{teamId:guid}")]
        [ProducesResponseType(typeof(Result<IReadOnlyList<PlayerDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
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
