using API.Contracts.Admin;
using Application.Common;
using Application.Features.Admin.DTOs;
using Application.Features.Admin.Leagues.Commands.AddLeagueMember;
using Application.Features.Admin.Leagues.Commands.DeleteAdminLeague;
using Application.Features.Admin.Leagues.Commands.RegenerateInviteCode;
using Application.Features.Admin.Leagues.Commands.RemoveLeagueMember;
using Application.Features.Admin.Leagues.Commands.UpdateAdminLeague;
using Application.Features.Admin.Leagues.Commands.UpdateLeagueMember;
using Application.Features.Admin.Leagues.Queries.GetAdminLeagueById;
using Application.Features.Admin.Leagues.Queries.GetAdminLeagueMembers;
using Application.Features.Admin.Leagues.Queries.GetAdminLeagues;
using Application.Features.Leagues.Commands.RecalculateStandings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/leagues")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminLeaguesController : BaseApiController
    {
        private readonly ISender _mediator;

        public AdminLeaguesController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<Result<PagedResult<AdminLeagueListDto>>>> GetLeagues(
            [FromQuery] Guid? tournamentId,
            [FromQuery] Guid? ownerId,
            [FromQuery] string? search,
            [FromQuery] bool? isPublic,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var query = new GetAdminLeaguesQuery(tournamentId, ownerId, search, isPublic, page, pageSize);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        [HttpGet("{leagueId:guid}")]
        public async Task<ActionResult<Result<AdminLeagueDto>>> GetLeagueById(
            Guid leagueId,
            CancellationToken ct = default)
        {
            var query = new GetAdminLeagueByIdQuery(leagueId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        [HttpPut("{leagueId:guid}")]
        public async Task<ActionResult<Result<AdminLeagueDto>>> UpdateLeague(
            Guid leagueId,
            [FromBody] UpdateAdminLeagueRequest request,
            CancellationToken ct = default)
        {
            var command = new UpdateAdminLeagueCommand(
                leagueId,
                request.Name,
                request.Description,
                request.IsPublic,
                request.IsGlobal,
                request.MaxMembers,
                request.ImageUrl
            );
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpDelete("{leagueId:guid}")]
        public async Task<ActionResult<Result<bool>>> DeleteLeague(
            Guid leagueId,
            CancellationToken ct = default)
        {
            var command = new DeleteAdminLeagueCommand(leagueId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPost("{leagueId:guid}/invite-code/regenerate")]
        public async Task<ActionResult<Result<string>>> RegenerateInviteCode(
            Guid leagueId,
            CancellationToken ct = default)
        {
            var command = new RegenerateInviteCodeCommand(leagueId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpGet("{leagueId:guid}/members")]
        public async Task<ActionResult<Result<IReadOnlyList<AdminLeagueMemberDto>>>> GetLeagueMembers(
            Guid leagueId,
            CancellationToken ct = default)
        {
            var query = new GetAdminLeagueMembersQuery(leagueId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        [HttpPost("{leagueId:guid}/members")]
        public async Task<ActionResult<Result<Guid>>> AddLeagueMember(
            Guid leagueId,
            [FromBody] AddLeagueMemberRequest request,
            CancellationToken ct = default)
        {
            var command = new AddLeagueMemberCommand(leagueId, request.UserId, request.IsAdmin);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpDelete("{leagueId:guid}/members/{userId:guid}")]
        public async Task<ActionResult<Result<bool>>> RemoveLeagueMember(
            Guid leagueId,
            Guid userId,
            CancellationToken ct = default)
        {
            var command = new RemoveLeagueMemberCommand(leagueId, userId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPut("{leagueId:guid}/members/{userId:guid}")]
        public async Task<ActionResult<Result<bool>>> UpdateLeagueMember(
            Guid leagueId,
            Guid userId,
            [FromBody] UpdateLeagueMemberRequest request,
            CancellationToken ct = default)
        {
            var command = new UpdateLeagueMemberCommand(leagueId, userId, request.IsAdmin, request.IsMuted);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPost("{leagueId:guid}/standings/recalculate")]
        public async Task<ActionResult<Result<bool>>> RecalculateStandings(
            Guid leagueId,
            CancellationToken ct = default)
        {
            var command = new RecalculateStandingsCommand(leagueId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
