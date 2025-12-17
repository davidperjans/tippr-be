using API.Contracts.Leagues;
using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.Commands.CreateLeague;
using Application.Features.Leagues.Commands.JoinLeague;
using Application.Features.Leagues.Commands.UpdateLeagueSettings;
using Application.Features.Leagues.DTOs;
using Application.Features.Leagues.Queries.GetLeague;
using Application.Features.Leagues.Queries.GetUserLeagues;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/leagues")]
    [Authorize]
    public class LeaguesController : BaseApiController
    {
        private readonly ISender _mediator;
        private readonly ICurrentUser _currentUser;
        public LeaguesController(ISender mediator, ICurrentUser currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        [HttpPost]
        public async Task<ActionResult<Result<Guid>>> Create([FromBody] CreateLeagueRequest request, CancellationToken ct)
        {
            var command = new CreateLeagueCommand(
                request.Name, 
                request.Description, 
                request.TournamentId, 
                _currentUser.UserId, 
                request.IsPublic, 
                request.MaxMembers, 
                request.ImageUrl
            );

            var result = await _mediator.Send(command, ct);

            return FromResult(result);
        }

        [HttpGet]
        public async Task<ActionResult<Result<IReadOnlyList<LeagueDto>>>> GetUserLeagues(CancellationToken ct)
        {
            var query = new GetUserLeaguesQuery(_currentUser.UserId);
            var result = await _mediator.Send(query, ct);

            return FromResult(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Result<LeagueDto>>> GetLeagueById(Guid id, CancellationToken ct)
        {
            var query = new GetLeagueQuery(id);
            var result = await _mediator.Send(query, ct);

            return FromResult(result);
        }

        [HttpPost("{id:guid}/join")]
        public async Task<ActionResult<Result<bool>>> JoinLeague(Guid id, [FromBody] JoinLeagueRequest request, CancellationToken ct)
        {
            var command = new JoinLeagueCommand(id, _currentUser.UserId, request.InviteCode);
            var result = await _mediator.Send(command, ct);

            return FromResult(result);
        }

        [HttpPut("{id:guid}/settings")]
        public async Task<ActionResult<Result<LeagueSettingsDto>>> UpdateLeagueSettings(Guid id, [FromBody] UpdateLeagueSettingsRequest request, CancellationToken ct)
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
    }
}
