using API.Contracts.Admin;
using Application.Common;
using Application.Features.Admin.DTOs;
using Application.Features.Admin.Teams.Commands.BulkCreateTeams;
using Application.Features.Admin.Teams.Commands.CreateTeam;
using Application.Features.Admin.Teams.Commands.DeleteTeam;
using Application.Features.Admin.Teams.Commands.UpdateTeam;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/teams")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminTeamsController : BaseApiController
    {
        private readonly ISender _mediator;

        public AdminTeamsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<Result<Guid>>> CreateTeam(
            [FromBody] CreateTeamRequest request,
            CancellationToken ct = default)
        {
            var command = new CreateTeamCommand(
                request.TournamentId,
                request.Name,
                request.Code,
                request.FlagUrl,
                request.GroupName,
                request.FifaRank,
                request.FifaPoints,
                request.ApiFootballId
            );
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPut("{teamId:guid}")]
        public async Task<ActionResult<Result<AdminTeamDto>>> UpdateTeam(
            Guid teamId,
            [FromBody] UpdateTeamRequest request,
            CancellationToken ct = default)
        {
            var command = new UpdateTeamCommand(
                teamId,
                request.Name,
                request.Code,
                request.FlagUrl,
                request.GroupName,
                request.FifaRank,
                request.FifaPoints,
                request.ApiFootballId
            );
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpDelete("{teamId:guid}")]
        public async Task<ActionResult<Result<bool>>> DeleteTeam(
            Guid teamId,
            CancellationToken ct = default)
        {
            var command = new DeleteTeamCommand(teamId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<Result<BulkCreateTeamsResult>>> BulkCreateTeams(
            [FromBody] BulkCreateTeamsRequest request,
            CancellationToken ct = default)
        {
            var teams = request.Teams.Select(t => new BulkTeamItem(
                t.Name,
                t.Code,
                t.FlagUrl,
                t.GroupName,
                t.FifaRank,
                t.FifaPoints,
                t.ApiFootballId
            )).ToList();

            var command = new BulkCreateTeamsCommand(request.TournamentId, teams);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
