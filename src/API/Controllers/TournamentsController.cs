using API.Contracts.Errors;
using Application.Common;
using Application.Features.Tournaments.Commands.CreateTournament;
using Application.Features.Tournaments.DTOs;
using Application.Features.Tournaments.Queries.GetAllTournaments;
using Application.Features.Tournaments.Queries.GetTournamentById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Manages football tournaments (e.g., World Cup, Euro, Champions League).
    /// Tournaments are the top-level container for teams, matches, and bonus questions.
    /// </summary>
    /// <remarks>
    /// <para><b>Domain Relationships:</b></para>
    /// <list type="bullet">
    ///   <item><description>Tournament contains Teams (via TournamentTeam join)</description></item>
    ///   <item><description>Tournament contains Matches</description></item>
    ///   <item><description>Tournament contains BonusQuestions</description></item>
    ///   <item><description>Leagues reference a Tournament for their prediction scope</description></item>
    /// </list>
    /// </remarks>
    [ApiController]
    [Route("api/tournaments")]
    [Produces("application/json")]
    public class TournamentsController : BaseApiController
    {
        private readonly ISender _mediator;

        public TournamentsController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all tournaments, optionally filtered to only active ones.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/tournaments?onlyActive=true
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
        ///       "name": "FIFA World Cup 2026",
        ///       "startDate": "2026-06-11",
        ///       "endDate": "2026-07-19",
        ///       "isActive": true,
        ///       "logoUrl": "https://example.com/wc2026.png"
        ///     }
        ///   ],
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="onlyActive">If true, returns only tournaments where IsActive = true</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of tournaments matching the filter criteria</returns>
        /// <response code="200">Returns the list of tournaments</response>
        /// <response code="401">JWT token is missing or invalid</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(Result<IReadOnlyList<TournamentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<IReadOnlyList<TournamentDto>>>> GetAll(
            [FromQuery] bool onlyActive = false,
            CancellationToken ct = default)
        {
            var query = new GetAllTournamentsQuery(onlyActive);
            var result = await _mediator.Send(query);
            return FromResult(result);
        }

        /// <summary>
        /// Retrieves a specific tournament by its unique identifier.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/tournaments/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "name": "FIFA World Cup 2026",
        ///     "startDate": "2026-06-11",
        ///     "endDate": "2026-07-19",
        ///     "isActive": true,
        ///     "logoUrl": "https://example.com/wc2026.png"
        ///   },
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">The unique identifier of the tournament</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The tournament details</returns>
        /// <response code="200">Returns the tournament</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="404">Tournament not found</response>
        [HttpGet("{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(Result<TournamentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<TournamentDto>>> GetById(Guid id, CancellationToken ct)
        {
            var query = new GetTournamentByIdQuery(id);
            var result = await _mediator.Send(query);
            return FromResult(result);
        }

        /// <summary>
        /// Creates a new tournament (Admin only).
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required (Admin role)</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// POST /api/tournaments
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: application/json
        ///
        /// {
        ///   "name": "FIFA World Cup 2026",
        ///   "startDate": "2026-06-11",
        ///   "endDate": "2026-07-19",
        ///   "isActive": true,
        ///   "logoUrl": "https://example.com/wc2026.png"
        /// }
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="command">The tournament creation details</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The ID of the newly created tournament</returns>
        /// <response code="200">Returns the ID of the created tournament</response>
        /// <response code="400">Validation error in request body</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User does not have Admin role</response>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Result<Guid>>> Create(CreateTournamentCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command);
            return FromResult(result);
        }
    }
}
