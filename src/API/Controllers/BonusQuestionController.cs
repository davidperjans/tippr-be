using API.Contracts.BonusQuestions;
using API.Contracts.Errors;
using Application.Common;
using Application.Features.BonusQuestions.Commands.CreateBonusQuestion;
using Application.Features.BonusQuestions.Commands.ResolveBonusQuestion;
using Application.Features.BonusQuestions.Queries.GetBonusQuestionsByTournament;
using Application.Features.Predictions.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Manages bonus questions for tournaments (e.g., "Who will win?", "Top scorer?").
    /// </summary>
    /// <remarks>
    /// <para><b>Domain Relationships:</b></para>
    /// <list type="bullet">
    ///   <item><description>BonusQuestions belong to a Tournament</description></item>
    ///   <item><description>BonusPredictions reference BonusQuestions and are scoped to a League</description></item>
    ///   <item><description>Answers can be Teams (for bracket predictions) or free text (for top scorer)</description></item>
    /// </list>
    ///
    /// <para><b>Question Types:</b></para>
    /// <list type="bullet">
    ///   <item><description>Winner: Which team will win the tournament</description></item>
    ///   <item><description>TopScorer: Which player will score the most goals</description></item>
    ///   <item><description>RoundOf16Team: Which teams will reach round of 16</description></item>
    ///   <item><description>QuarterFinalTeam, SemiFinalTeam, FinalTeam: Bracket predictions</description></item>
    ///   <item><description>MostGoalsGroup, MostConcededGroup: Group stage statistics</description></item>
    /// </list>
    /// </remarks>
    [ApiController]
    [Route("api/bonus-questions")]
    [Authorize]
    [Produces("application/json")]
    public class BonusQuestionController : BaseApiController
    {
        private readonly ISender _mediator;

        public BonusQuestionController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all bonus questions for a tournament.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/bonus-questions?tournamentId=3fa85f64-5717-4562-b3fc-2c963f66afa6
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
        ///       "tournamentId": "...",
        ///       "questionType": "Winner",
        ///       "question": "Who will win the World Cup?",
        ///       "points": 10,
        ///       "isResolved": false,
        ///       "answerTeamId": null,
        ///       "answerText": null
        ///     },
        ///     {
        ///       "id": "...",
        ///       "tournamentId": "...",
        ///       "questionType": "TopScorer",
        ///       "question": "Who will be the top scorer?",
        ///       "points": 5,
        ///       "isResolved": false,
        ///       "answerTeamId": null,
        ///       "answerText": null
        ///     }
        ///   ],
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="tournamentId">The tournament ID to get questions for</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of bonus questions for the tournament</returns>
        /// <response code="200">Returns the list of bonus questions</response>
        /// <response code="401">JWT token is missing or invalid</response>
        [HttpGet]
        [ProducesResponseType(typeof(Result<IReadOnlyList<BonusQuestionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<IReadOnlyList<BonusQuestionDto>>>> GetByTournament(
            [FromQuery] Guid tournamentId,
            CancellationToken ct)
        {
            var query = new GetBonusQuestionsByTournamentQuery(tournamentId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Creates a new bonus question for a tournament (Admin only).
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required (Admin role)</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// POST /api/bonus-questions
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: application/json
        ///
        /// {
        ///   "tournamentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "questionType": "Winner",
        ///   "question": "Who will win the World Cup 2026?",
        ///   "points": 10
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
        /// <param name="request">The bonus question details</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The ID of the created bonus question</returns>
        /// <response code="200">Returns the ID of the created bonus question</response>
        /// <response code="400">Validation error (invalid question type or points)</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User does not have Admin role</response>
        /// <response code="404">Tournament not found</response>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<Guid>>> Create(
            [FromBody] CreateBonusQuestionRequest request,
            CancellationToken ct)
        {
            var command = new CreateBonusQuestionCommand(
                request.TournamentId,
                request.QuestionType,
                request.Question,
                request.Points
            );

            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Resolves a bonus question with the correct answer and awards points (Admin only).
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required (Admin role)</para>
        /// <para><b>Side Effects:</b></para>
        /// <list type="bullet">
        ///   <item><description>Marks the question as resolved with the correct answer</description></item>
        ///   <item><description>Awards points to all users who predicted correctly</description></item>
        ///   <item><description>Updates league standings for affected leagues</description></item>
        /// </list>
        ///
        /// <para><b>Example Request (Team answer):</b></para>
        /// <code>
        /// PUT /api/bonus-questions/3fa85f64-5717-4562-b3fc-2c963f66afa6/resolve
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: application/json
        ///
        /// {
        ///   "answerTeamId": "4fb96f75-6818-5673-c4gd-3d074g77bfb7",
        ///   "answerText": null
        /// }
        /// </code>
        ///
        /// <para><b>Example Request (Text answer):</b></para>
        /// <code>
        /// PUT /api/bonus-questions/3fa85f64-5717-4562-b3fc-2c963f66afa6/resolve
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: application/json
        ///
        /// {
        ///   "answerTeamId": null,
        ///   "answerText": "Kylian Mbappe"
        /// }
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": 15,
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">The bonus question ID to resolve</param>
        /// <param name="request">The correct answer (team ID or text)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The number of users who received points</returns>
        /// <response code="200">Question resolved and points awarded</response>
        /// <response code="400">Question already resolved or invalid answer</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User does not have Admin role</response>
        /// <response code="404">Bonus question not found</response>
        [HttpPut("{id:guid}/resolve")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<int>>> Resolve(
            [FromRoute] Guid id,
            [FromBody] ResolveBonusQuestionRequest request,
            CancellationToken ct)
        {
            var command = new ResolveBonusQuestionCommand(
                id,
                request.AnswerTeamId,
                request.AnswerText
            );

            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
