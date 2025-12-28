using API.Contracts.BonusQuestions;
using API.Contracts.Errors;
using Application.Common;
using Application.Features.BonusQuestions.Commands.SubmitBonusPrediction;
using Application.Features.BonusQuestions.Queries.GetUserBonusPredictions;
using Application.Features.Predictions.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Manages user predictions for bonus questions within leagues.
    /// </summary>
    /// <remarks>
    /// <para><b>Domain Relationships:</b></para>
    /// <list type="bullet">
    ///   <item><description>BonusPredictions are unique per League + BonusQuestion + User combination</description></item>
    ///   <item><description>BonusPredictions reference a BonusQuestion (provides the question and correct answer)</description></item>
    ///   <item><description>Points are awarded when the BonusQuestion is resolved by an admin</description></item>
    /// </list>
    ///
    /// <para><b>Answer Types:</b></para>
    /// <list type="bullet">
    ///   <item><description>Team predictions: Use answerTeamId (e.g., for "Who will win?")</description></item>
    ///   <item><description>Text predictions: Use answerText (e.g., for "Top scorer?")</description></item>
    /// </list>
    /// </remarks>
    [ApiController]
    [Route("api/bonus-predictions")]
    [Authorize]
    [Produces("application/json")]
    public class BonusPredictionController : BaseApiController
    {
        private readonly ISender _mediator;

        public BonusPredictionController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Submits a prediction for a bonus question within a league.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Rules:</b></para>
        /// <list type="bullet">
        ///   <item><description>User must be a member of the specified league</description></item>
        ///   <item><description>BonusQuestion must belong to the league's tournament</description></item>
        ///   <item><description>Cannot submit after the bonus question is resolved</description></item>
        ///   <item><description>Submitting again will update the existing prediction</description></item>
        /// </list>
        ///
        /// <para><b>Example Request (Team answer):</b></para>
        /// <code>
        /// POST /api/bonus-predictions
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: application/json
        ///
        /// {
        ///   "leagueId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "bonusQuestionId": "4fb96f75-6818-5673-c4gd-3d074g77bfb7",
        ///   "answerTeamId": "5gc07h86-7929-6784-d5he-4e185h88cgc8",
        ///   "answerText": null
        /// }
        /// </code>
        ///
        /// <para><b>Example Request (Text answer):</b></para>
        /// <code>
        /// POST /api/bonus-predictions
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: application/json
        ///
        /// {
        ///   "leagueId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "bonusQuestionId": "4fb96f75-6818-5673-c4gd-3d074g77bfb7",
        ///   "answerTeamId": null,
        ///   "answerText": "Kylian Mbappe"
        /// }
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": "6hd18i97-8030-7895-e6if-5f296i99dhd9",
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="request">The bonus prediction details</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The ID of the created/updated bonus prediction</returns>
        /// <response code="200">Bonus prediction submitted successfully</response>
        /// <response code="400">Question already resolved or invalid answer type</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User is not a member of the league</response>
        /// <response code="404">League or bonus question not found</response>
        [HttpPost]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<Guid>>> Submit(
            [FromBody] SubmitBonusPredictionRequest request,
            CancellationToken ct)
        {
            var command = new SubmitBonusPredictionCommand(
                request.LeagueId,
                request.BonusQuestionId,
                request.AnswerTeamId,
                request.AnswerText
            );

            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Retrieves the current user's bonus predictions for a league.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/bonus-predictions?leagueId=3fa85f64-5717-4562-b3fc-2c963f66afa6
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
        ///       "bonusQuestionId": "...",
        ///       "question": "Who will win the World Cup?",
        ///       "questionType": "Winner",
        ///       "answerTeamId": "...",
        ///       "answerTeamName": "Brazil",
        ///       "answerText": null,
        ///       "points": 10,
        ///       "isCorrect": true,
        ///       "isResolved": true
        ///     },
        ///     {
        ///       "id": "...",
        ///       "bonusQuestionId": "...",
        ///       "question": "Who will be the top scorer?",
        ///       "questionType": "TopScorer",
        ///       "answerTeamId": null,
        ///       "answerTeamName": null,
        ///       "answerText": "Kylian Mbappe",
        ///       "points": null,
        ///       "isCorrect": null,
        ///       "isResolved": false
        ///     }
        ///   ],
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="leagueId">The league ID to get bonus predictions for</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of user's bonus predictions with scoring details</returns>
        /// <response code="200">Returns the user's bonus predictions</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User is not a member of the league</response>
        [HttpGet]
        [ProducesResponseType(typeof(Result<List<BonusPredictionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Result<List<BonusPredictionDto>>>> GetMine(
            [FromQuery] Guid leagueId,
            CancellationToken ct)
        {
            var query = new GetUserBonusPredictionsQuery(leagueId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }
    }
}
