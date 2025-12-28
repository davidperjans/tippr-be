using API.Contracts.Errors;
using Application.Common;
using Application.Features.Teams.DTOs;
using Application.Features.Teams.Queries.GetTeam;
using Application.Features.Teams.Queries.GetTeamsByTournament;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Provides read-only access to teams participating in tournaments.
    /// </summary>
    /// <remarks>
    /// <para><b>Domain Relationships:</b></para>
    /// <list type="bullet">
    ///   <item><description>Teams belong to Tournaments (via TournamentTeam join)</description></item>
    ///   <item><description>Teams participate in Matches as home or away team</description></item>
    ///   <item><description>Teams have Players</description></item>
    ///   <item><description>Teams can be set as user's FavoriteTeam</description></item>
    ///   <item><description>Teams can be answers to BonusQuestions (e.g., "Who will win?")</description></item>
    /// </list>
    /// </remarks>
    [ApiController]
    [Route("api/teams")]
    [Authorize]
    [Produces("application/json")]
    public sealed class TeamsController : BaseApiController
    {
        private readonly ISender _mediator;

        public TeamsController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all teams participating in a specific tournament.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/teams?tournamentId=3fa85f64-5717-4562-b3fc-2c963f66afa6
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
        ///       "name": "Brazil",
        ///       "shortName": "BRA",
        ///       "logoUrl": "https://example.com/brazil.png",
        ///       "group": "A"
        ///     },
        ///     {
        ///       "id": "4fb96f75-6818-5673-c4gd-3d074g77bfb7",
        ///       "name": "Germany",
        ///       "shortName": "GER",
        ///       "logoUrl": "https://example.com/germany.png",
        ///       "group": "A"
        ///     }
        ///   ],
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="tournamentId">The tournament ID to filter teams by (required)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of teams in the specified tournament</returns>
        /// <response code="200">Returns the list of teams</response>
        /// <response code="401">JWT token is missing or invalid</response>
        [HttpGet]
        [ProducesResponseType(typeof(Result<IReadOnlyList<TeamDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<IReadOnlyList<TeamDto>>>> GetTeams(
            [FromQuery] Guid tournamentId,
            CancellationToken ct)
        {
            var query = new GetTeamsByTournamentQuery(tournamentId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Retrieves a specific team by its unique identifier.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/teams/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "name": "Brazil",
        ///     "shortName": "BRA",
        ///     "logoUrl": "https://example.com/brazil.png",
        ///     "group": "A"
        ///   },
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">The unique identifier of the team</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The team details</returns>
        /// <response code="200">Returns the team</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="404">Team not found</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Result<TeamDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<TeamDto>>> GetTeam(
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            var query = new GetTeamQuery(id);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }
    }
}
