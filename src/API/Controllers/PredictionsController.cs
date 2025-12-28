using API.Contracts.Errors;
using API.Contracts.Predictions;
using Application.Common;
using Application.Features.Predictions.Commands.BulkSubmitPredictions;
using Application.Features.Predictions.Commands.SubmitPrediction;
using Application.Features.Predictions.Commands.UpdatePrediction;
using Application.Features.Predictions.DTOs;
using Application.Features.Predictions.Queries.GetPrediction;
using Application.Features.Predictions.Queries.GetUserPredictions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Manages match predictions for users within leagues.
    /// </summary>
    /// <remarks>
    /// <para><b>Domain Relationships:</b></para>
    /// <list type="bullet">
    ///   <item><description>Predictions are unique per League + Match + User combination</description></item>
    ///   <item><description>Predictions belong to a League (determines scoring settings)</description></item>
    ///   <item><description>Predictions reference a Match (provides actual result for scoring)</description></item>
    ///   <item><description>Points are calculated when Match status becomes "Finished"</description></item>
    /// </list>
    ///
    /// <para><b>Deadline Rules:</b></para>
    /// <list type="bullet">
    ///   <item><description>Predictions must be submitted before the league's deadline (based on PredictionMode)</description></item>
    ///   <item><description>BeforeKickoff: Deadline is X minutes before match kickoff</description></item>
    ///   <item><description>BeforeMatchday: Deadline is before first match of the day</description></item>
    ///   <item><description>Late predictions are rejected unless AllowLateEdits is enabled</description></item>
    /// </list>
    /// </remarks>
    [ApiController]
    [Route("api/predictions")]
    [Authorize]
    [Produces("application/json")]
    public class PredictionsController : BaseApiController
    {
        private readonly ISender _mediator;

        public PredictionsController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Submits a new prediction for a match within a league.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Rules:</b></para>
        /// <list type="bullet">
        ///   <item><description>User must be a member of the specified league</description></item>
        ///   <item><description>Match must belong to the league's tournament</description></item>
        ///   <item><description>Must be submitted before the league's deadline</description></item>
        ///   <item><description>Cannot create duplicate predictions (use PUT to update)</description></item>
        /// </list>
        /// <para><b>Side Effects:</b> Creates a Prediction record; points calculated when match finishes</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// POST /api/predictions
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: application/json
        ///
        /// {
        ///   "leagueId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "matchId": "4fb96f75-6818-5673-c4gd-3d074g77bfb7",
        ///   "homeScore": 2,
        ///   "awayScore": 1
        /// }
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": "5gc07h86-7929-6784-d5he-4e185h88cgc8",
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="request">The prediction details</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The ID of the created prediction</returns>
        /// <response code="200">Prediction created successfully</response>
        /// <response code="400">Deadline has passed or validation error</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User is not a member of the league</response>
        /// <response code="404">League or match not found</response>
        /// <response code="409">Prediction already exists for this match</response>
        [HttpPost]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Result<Guid>>> Submit(
            [FromBody] SubmitPredictionRequest request,
            CancellationToken ct)
        {
            var command = new SubmitPredictionCommand(
                request.LeagueId,
                request.MatchId,
                request.HomeScore,
                request.AwayScore
            );

            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Submits multiple predictions at once for a league.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Behavior:</b></para>
        /// <list type="bullet">
        ///   <item><description>Creates new predictions or updates existing ones</description></item>
        ///   <item><description>Processes all predictions in a single transaction</description></item>
        ///   <item><description>Returns count of created, updated, and failed predictions</description></item>
        ///   <item><description>Individual failures do not roll back successful predictions</description></item>
        /// </list>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// POST /api/predictions/bulk
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: application/json
        ///
        /// {
        ///   "leagueId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "predictions": [
        ///     { "matchId": "...", "homeScore": 2, "awayScore": 1 },
        ///     { "matchId": "...", "homeScore": 0, "awayScore": 0 },
        ///     { "matchId": "...", "homeScore": 3, "awayScore": 2 }
        ///   ]
        /// }
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "created": 2,
        ///     "updated": 1,
        ///     "failed": 0,
        ///     "errors": []
        ///   },
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="request">The league ID and list of predictions</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Summary of created, updated, and failed predictions</returns>
        /// <response code="200">Bulk operation completed (check result for individual failures)</response>
        /// <response code="400">Validation error in request structure</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User is not a member of the league</response>
        /// <response code="404">League not found</response>
        [HttpPost("bulk")]
        [ProducesResponseType(typeof(Result<BulkSubmitPredictionsResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<BulkSubmitPredictionsResult>>> BulkSubmit(
            [FromBody] BulkSubmitPredictionsRequest request,
            CancellationToken ct)
        {
            var predictions = request.Predictions
                .Select(p => new PredictionItem(p.MatchId, p.HomeScore, p.AwayScore))
                .ToList();

            var command = new BulkSubmitPredictionsCommand(request.LeagueId, predictions);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Updates an existing prediction's score.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Rules:</b></para>
        /// <list type="bullet">
        ///   <item><description>User must own the prediction</description></item>
        ///   <item><description>Must be updated before the league's deadline</description></item>
        ///   <item><description>Cannot update after match has started (unless AllowLateEdits is enabled)</description></item>
        /// </list>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// PUT /api/predictions/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: application/json
        ///
        /// {
        ///   "homeScore": 3,
        ///   "awayScore": 1
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
        /// <param name="id">The prediction ID to update</param>
        /// <param name="request">The new score prediction</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if update was successful</returns>
        /// <response code="200">Prediction updated successfully</response>
        /// <response code="400">Deadline has passed or match already started</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User does not own this prediction</response>
        /// <response code="404">Prediction not found</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<bool>>> Update(
            [FromRoute] Guid id,
            [FromBody] UpdatePredictionRequest request,
            CancellationToken ct)
        {
            var command = new UpdatePredictionCommand(
                id,
                request.HomeScore,
                request.AwayScore
            );

            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Retrieves all predictions for the current user in a specific league.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/predictions?leagueId=3fa85f64-5717-4562-b3fc-2c963f66afa6
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
        ///       "matchId": "...",
        ///       "homeTeam": "Brazil",
        ///       "awayTeam": "Germany",
        ///       "homeScore": 2,
        ///       "awayScore": 1,
        ///       "points": 3,
        ///       "isCorrectScore": true,
        ///       "isCorrectOutcome": true,
        ///       "matchStatus": "Finished",
        ///       "actualHomeScore": 2,
        ///       "actualAwayScore": 1
        ///     }
        ///   ],
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="leagueId">The league ID to get predictions for</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of user's predictions with scoring details</returns>
        /// <response code="200">Returns the user's predictions</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User is not a member of the league</response>
        [HttpGet]
        [ProducesResponseType(typeof(Result<List<PredictionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Result<List<PredictionDto>>>> GetMine(
            [FromQuery] Guid leagueId,
            CancellationToken ct)
        {
            var query = new GetUserPredictionsQuery(leagueId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Retrieves the current user's prediction for a specific match in a league.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/predictions/match/3fa85f64-5717-4562-b3fc-2c963f66afa6?leagueId=4fb96f75-6818-5673-c4gd-3d074g77bfb7
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "id": "...",
        ///     "matchId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "homeScore": 2,
        ///     "awayScore": 1,
        ///     "points": null,
        ///     "matchStatus": "Scheduled"
        ///   },
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="matchId">The match ID</param>
        /// <param name="leagueId">The league ID</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The prediction if it exists, null otherwise</returns>
        /// <response code="200">Returns the prediction or null if not found</response>
        /// <response code="401">JWT token is missing or invalid</response>
        [HttpGet("match/{matchId:guid}")]
        [ProducesResponseType(typeof(Result<PredictionDto?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<PredictionDto?>>> GetForMatch(
            [FromRoute] Guid matchId,
            [FromQuery] Guid leagueId,
            CancellationToken ct)
        {
            var query = new GetPredictionQuery(leagueId, matchId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }
    }
}
