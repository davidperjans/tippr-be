using API.Contracts.Admin;
using Application.Common;
using Application.Features.Admin.DTOs;
using Application.Features.Admin.Standings.Commands.RecalculateTournamentStandings;
using Application.Features.Admin.Tournaments.Commands.ActivateTournament;
using Application.Features.Admin.Tournaments.Commands.DeactivateTournament;
using Application.Features.Admin.Tournaments.Commands.DeleteTournament;
using Application.Features.Admin.Tournaments.Commands.UpdateTournament;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/tournaments")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminTournamentsController : BaseApiController
    {
        private readonly ISender _mediator;

        public AdminTournamentsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpPut("{tournamentId:guid}")]
        public async Task<ActionResult<Result<AdminTournamentDto>>> UpdateTournament(
            Guid tournamentId,
            [FromBody] UpdateTournamentRequest request,
            CancellationToken ct = default)
        {
            var command = new UpdateTournamentCommand(
                tournamentId,
                request.Name,
                request.Year,
                request.Type,
                request.StartDate,
                request.EndDate,
                request.LogoUrl
            );
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPost("{tournamentId:guid}/activate")]
        public async Task<ActionResult<Result<bool>>> ActivateTournament(
            Guid tournamentId,
            CancellationToken ct = default)
        {
            var command = new ActivateTournamentCommand(tournamentId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPost("{tournamentId:guid}/deactivate")]
        public async Task<ActionResult<Result<bool>>> DeactivateTournament(
            Guid tournamentId,
            CancellationToken ct = default)
        {
            var command = new DeactivateTournamentCommand(tournamentId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpDelete("{tournamentId:guid}")]
        public async Task<ActionResult<Result<bool>>> DeleteTournament(
            Guid tournamentId,
            CancellationToken ct = default)
        {
            var command = new DeleteTournamentCommand(tournamentId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPost("{tournamentId:guid}/standings/recalculate")]
        public async Task<ActionResult<Result<RecalculateTournamentStandingsResult>>> RecalculateTournamentStandings(
            Guid tournamentId,
            CancellationToken ct = default)
        {
            var command = new RecalculateTournamentStandingsCommand(tournamentId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
