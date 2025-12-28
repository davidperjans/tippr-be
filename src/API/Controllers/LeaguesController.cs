using API.Contracts.Errors;
using API.Contracts.Leagues;
using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.Commands.CreateLeague;
using Application.Features.Leagues.Commands.DeleteLeague;
using Application.Features.Leagues.Commands.JoinLeague;
using Application.Features.Leagues.Commands.LeaveLeague;
using Application.Features.Leagues.Commands.RecalculateStandings;
using Application.Features.Leagues.Commands.UpdateLeagueSettings;
using Application.Features.Leagues.DTOs;
using Application.Features.Leagues.Queries.GetLeague;
using Application.Features.Leagues.Queries.GetLeagueStandings;
using Application.Features.Leagues.Queries.GetUserLeagues;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Manages prediction leagues where users compete against each other.
    /// </summary>
    /// <remarks>
    /// <para><b>Domain Relationships:</b></para>
    /// <list type="bullet">
    ///   <item><description>League belongs to a Tournament (defines which matches are available for predictions)</description></item>
    ///   <item><description>League has Members (users who joined)</description></item>
    ///   <item><description>League has an Owner (the user who created it)</description></item>
    ///   <item><description>Predictions are scoped to League + Match + User (unique constraint)</description></item>
    ///   <item><description>BonusPredictions are scoped to League + BonusQuestion + User</description></item>
    ///   <item><description>LeagueStandings track points per user in a league</description></item>
    /// </list>
    ///
    /// <para><b>Access Control:</b></para>
    /// <list type="bullet">
    ///   <item><description>Public leagues: Anyone can join without invite code</description></item>
    ///   <item><description>Private leagues: Require invite code to join</description></item>
    ///   <item><description>Owner-only operations: Update settings, delete league</description></item>
    /// </list>
    /// </remarks>
    [ApiController]
    [Route("api/leagues")]
    [Authorize]
    [Produces("application/json")]
    public class LeaguesController : BaseApiController
    {
        private readonly ISender _mediator;
        private readonly ICurrentUser _currentUser;

        public LeaguesController(ISender mediator, ICurrentUser currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Creates a new prediction league.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Side Effects:</b></para>
        /// <list type="bullet">
        ///   <item><description>Creates the league with default scoring settings</description></item>
        ///   <item><description>Automatically adds the creator as owner and first member</description></item>
        ///   <item><description>Generates a unique invite code for private leagues</description></item>
        /// </list>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// POST /api/leagues
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: application/json
        ///
        /// {
        ///   "name": "Office Champions",
        ///   "description": "Our office prediction league",
        ///   "tournamentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "isPublic": false,
        ///   "maxMembers": 50,
        ///   "imageUrl": "https://example.com/league.png"
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
        /// <param name="request">The league creation details</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The ID of the newly created league</returns>
        /// <response code="200">Returns the ID of the created league</response>
        /// <response code="400">Validation error (e.g., name too long, invalid tournament)</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="404">Tournament not found</response>
        [HttpPost]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<Guid>>> Create(
            [FromBody] CreateLeagueRequest request,
            CancellationToken ct)
        {
            var command = new CreateLeagueCommand(
                request.Name,
                request.Description,
                request.TournamentId,
                request.IsPublic,
                request.MaxMembers,
                request.ImageUrl
            );

            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Retrieves all leagues the current user is a member of.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/leagues
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
        ///       "name": "Office Champions",
        ///       "tournamentName": "FIFA World Cup 2026",
        ///       "memberCount": 12,
        ///       "isOwner": true,
        ///       "imageUrl": "https://example.com/league.png"
        ///     }
        ///   ],
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of leagues the user has joined</returns>
        /// <response code="200">Returns the user's leagues</response>
        /// <response code="401">JWT token is missing or invalid</response>
        [HttpGet]
        [ProducesResponseType(typeof(Result<IReadOnlyList<LeagueListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Result<IReadOnlyList<LeagueListDto>>>> GetUserLeagues(CancellationToken ct)
        {
            var query = new GetUserLeaguesQuery(_currentUser.UserId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Retrieves detailed information for a specific league.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Access:</b> User must be a member of the league to view details</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/leagues/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "name": "Office Champions",
        ///     "description": "Our office prediction league",
        ///     "tournament": { "id": "...", "name": "FIFA World Cup 2026" },
        ///     "owner": { "id": "...", "displayName": "John Doe" },
        ///     "memberCount": 12,
        ///     "maxMembers": 50,
        ///     "isPublic": false,
        ///     "inviteCode": "ABC123",
        ///     "settings": {
        ///       "predictionMode": "BeforeKickoff",
        ///       "deadlineMinutes": 15,
        ///       "pointsCorrectScore": 3,
        ///       "pointsCorrectOutcome": 1
        ///     }
        ///   },
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">The league ID</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The league details including settings and invite code</returns>
        /// <response code="200">Returns the league details</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User is not a member of this league</response>
        /// <response code="404">League not found</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Result<LeagueDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<LeagueDto>>> GetLeagueById(Guid id, CancellationToken ct)
        {
            var query = new GetLeagueQuery(id, _currentUser.UserId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Joins an existing league using an optional invite code.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Rules:</b></para>
        /// <list type="bullet">
        ///   <item><description>Public leagues: No invite code required</description></item>
        ///   <item><description>Private leagues: Valid invite code is required</description></item>
        ///   <item><description>Cannot join if already a member</description></item>
        ///   <item><description>Cannot join if league is at max capacity</description></item>
        /// </list>
        /// <para><b>Side Effects:</b> Creates a LeagueMember entry for the user</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// POST /api/leagues/3fa85f64-5717-4562-b3fc-2c963f66afa6/join
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: application/json
        ///
        /// {
        ///   "inviteCode": "ABC123"
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
        /// <param name="id">The league ID to join</param>
        /// <param name="request">The invite code (required for private leagues)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if successfully joined</returns>
        /// <response code="200">Successfully joined the league</response>
        /// <response code="400">Invalid invite code or league is full</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="404">League not found</response>
        /// <response code="409">User is already a member of this league</response>
        [HttpPost("{id:guid}/join")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Result<bool>>> JoinLeague(
            Guid id,
            [FromBody] JoinLeagueRequest request,
            CancellationToken ct)
        {
            var command = new JoinLeagueCommand(id, request.InviteCode);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Leaves a league the current user is a member of.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Rules:</b></para>
        /// <list type="bullet">
        ///   <item><description>League owner cannot leave (must delete or transfer ownership)</description></item>
        ///   <item><description>User must be a member to leave</description></item>
        /// </list>
        /// <para><b>Side Effects:</b></para>
        /// <list type="bullet">
        ///   <item><description>Removes LeagueMember entry</description></item>
        ///   <item><description>User's predictions and standings remain for historical purposes</description></item>
        /// </list>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// POST /api/leagues/3fa85f64-5717-4562-b3fc-2c963f66afa6/leave
        /// Authorization: Bearer &lt;access_token&gt;
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
        /// <param name="id">The league ID to leave</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if successfully left</returns>
        /// <response code="200">Successfully left the league</response>
        /// <response code="400">Owner cannot leave the league</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="404">League not found or user is not a member</response>
        [HttpPost("{id:guid}/leave")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<bool>>> LeaveLeague(Guid id, CancellationToken ct)
        {
            var command = new LeaveLeagueCommand(id);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Updates the settings for a league (Owner only).
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required (must be league owner)</para>
        /// <para><b>Partial Update:</b> All fields are optional; only provided values are updated</para>
        ///
        /// <para><b>Settings:</b></para>
        /// <list type="bullet">
        ///   <item><description>predictionMode: "BeforeKickoff" or "BeforeMatchday"</description></item>
        ///   <item><description>deadlineMinutes: Minutes before kickoff when predictions lock</description></item>
        ///   <item><description>pointsCorrectScore: Points for exact score prediction (default: 3)</description></item>
        ///   <item><description>pointsCorrectOutcome: Points for correct outcome only (default: 1)</description></item>
        ///   <item><description>pointsCorrectGoals: Points for correct total goals</description></item>
        ///   <item><description>Bonus points for bracket predictions (round of 16, quarters, etc.)</description></item>
        /// </list>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// PUT /api/leagues/3fa85f64-5717-4562-b3fc-2c963f66afa6/settings
        /// Authorization: Bearer &lt;access_token&gt;
        /// Content-Type: application/json
        ///
        /// {
        ///   "predictionMode": "BeforeKickoff",
        ///   "deadlineMinutes": 15,
        ///   "pointsCorrectScore": 3,
        ///   "pointsCorrectOutcome": 1,
        ///   "allowLateEdits": false
        /// }
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": {
        ///     "predictionMode": "BeforeKickoff",
        ///     "deadlineMinutes": 15,
        ///     "pointsCorrectScore": 3,
        ///     "pointsCorrectOutcome": 1,
        ///     "pointsCorrectGoals": 0,
        ///     "allowLateEdits": false
        ///   },
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">The league ID</param>
        /// <param name="request">The settings to update</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The updated settings</returns>
        /// <response code="200">Settings updated successfully</response>
        /// <response code="400">Invalid prediction mode or settings values</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User is not the owner of this league</response>
        /// <response code="404">League not found</response>
        [HttpPut("{id:guid}/settings")]
        [ProducesResponseType(typeof(Result<LeagueSettingsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<LeagueSettingsDto>>> UpdateLeagueSettings(
            Guid id,
            [FromBody] UpdateLeagueSettingsRequest request,
            CancellationToken ct)
        {
            if (!Enum.TryParse<PredictionMode>(request.PredictionMode, ignoreCase: true, out var mode))
                return FromResult(Result<LeagueSettingsDto>.Failure("invalid PredictionMode."));

            var cmd = new UpdateLeagueSettingsCommand(
                LeagueId: id,
                UserId: _currentUser.UserId,
                PredictionMode: mode,
                DeadlineMinutes: request.DeadlineMinutes,
                PointsCorrectScore: request.PointsCorrectScore,
                PointsCorrectOutcome: request.PointsCorrectOutcome,
                PointsCorrectGoals: request.PointsCorrectGoals,
                PointsRoundOf16Team: request.PointsRoundOf16Team,
                PointsQuarterFinalTeam: request.PointsQuarterFinalTeam,
                PointsSemiFinalTeam: request.PointsSemiFinalTeam,
                PointsFinalTeam: request.PointsFinalTeam,
                PointsTopScorer: request.PointsTopScorer,
                PointsWinner: request.PointsWinner,
                PointsMostGoalsGroup: request.PointsMostGoalsGroup,
                PointsMostConcededGroup: request.PointsMostConcededGroup,
                AllowLateEdits: request.AllowLateEdits
            );

            var result = await _mediator.Send(cmd, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Retrieves the current standings (leaderboard) for a league.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Access:</b> User must be a member of the league</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// GET /api/leagues/3fa85f64-5717-4562-b3fc-2c963f66afa6/standings
        /// Authorization: Bearer &lt;access_token&gt;
        /// </code>
        ///
        /// <para><b>Example Response:</b></para>
        /// <code>
        /// {
        ///   "isSuccess": true,
        ///   "data": [
        ///     {
        ///       "rank": 1,
        ///       "userId": "...",
        ///       "displayName": "John Doe",
        ///       "avatarUrl": "...",
        ///       "totalPoints": 45,
        ///       "correctScores": 5,
        ///       "correctOutcomes": 10,
        ///       "bonusPoints": 15
        ///     },
        ///     {
        ///       "rank": 2,
        ///       "userId": "...",
        ///       "displayName": "Jane Smith",
        ///       "avatarUrl": "...",
        ///       "totalPoints": 42,
        ///       "correctScores": 4,
        ///       "correctOutcomes": 12,
        ///       "bonusPoints": 10
        ///     }
        ///   ],
        ///   "error": null
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">The league ID</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Ordered list of members with their scores and ranks</returns>
        /// <response code="200">Returns the standings</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User is not a member of this league</response>
        /// <response code="404">League not found</response>
        [HttpGet("{id:guid}/standings")]
        [ProducesResponseType(typeof(Result<IReadOnlyList<LeagueStandingDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<IReadOnlyList<LeagueStandingDto>>>> GetStandings(
            Guid id,
            CancellationToken ct)
        {
            var query = new GetLeagueStandingsQuery(id, _currentUser.UserId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Triggers a full recalculation of standings for a league.
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required</para>
        /// <para><b>Use Case:</b> Data integrity checks or after manual corrections to predictions/matches</para>
        /// <para><b>Side Effects:</b> Recalculates all points for all members based on current match results</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// POST /api/leagues/3fa85f64-5717-4562-b3fc-2c963f66afa6/standings/recalculate
        /// Authorization: Bearer &lt;access_token&gt;
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
        /// <param name="id">The league ID</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if recalculation completed successfully</returns>
        /// <response code="200">Standings recalculated successfully</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="404">League not found</response>
        [HttpPost("{id:guid}/standings/recalculate")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<bool>>> RecalculateStandings(Guid id, CancellationToken ct)
        {
            var command = new RecalculateStandingsCommand(id);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Deletes a league and all associated data (Owner only).
        /// </summary>
        /// <remarks>
        /// <para><b>Auth:</b> JWT Bearer token required (must be league owner)</para>
        /// <para><b>Warning:</b> This is a destructive operation and cannot be undone</para>
        /// <para><b>Side Effects:</b> Deletes the league, all memberships, predictions, standings, and chat messages</para>
        ///
        /// <para><b>Example Request:</b></para>
        /// <code>
        /// DELETE /api/leagues/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer &lt;access_token&gt;
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
        /// <param name="id">The league ID to delete</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if deletion was successful</returns>
        /// <response code="200">League deleted successfully</response>
        /// <response code="401">JWT token is missing or invalid</response>
        /// <response code="403">User is not the owner of this league</response>
        /// <response code="404">League not found</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<bool>>> Delete(Guid id, CancellationToken ct)
        {
            var command = new DeleteLeagueCommand(id, _currentUser.UserId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
