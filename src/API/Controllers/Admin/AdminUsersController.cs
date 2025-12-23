using API.Contracts.Admin;
using Application.Common;
using Application.Features.Admin.DTOs;
using Application.Features.Admin.Users.Commands.BanUser;
using Application.Features.Admin.Users.Commands.UnbanUser;
using Application.Features.Admin.Users.Commands.UpdateAdminUser;
using Application.Features.Admin.Users.Commands.UpdateUserRole;
using Application.Features.Admin.Users.Queries.GetAdminUserById;
using Application.Features.Admin.Users.Queries.GetAdminUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminUsersController : BaseApiController
    {
        private readonly ISender _mediator;

        public AdminUsersController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<Result<PagedResult<AdminUserListDto>>>> GetUsers(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sort = null,
            CancellationToken ct = default)
        {
            var query = new GetAdminUsersQuery(search, page, pageSize, sort);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<Result<AdminUserDto>>> GetUserById(
            Guid userId,
            CancellationToken ct = default)
        {
            var query = new GetAdminUserByIdQuery(userId);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        [HttpPut("{userId:guid}")]
        public async Task<ActionResult<Result<AdminUserDto>>> UpdateUser(
            Guid userId,
            [FromBody] UpdateAdminUserRequest request,
            CancellationToken ct = default)
        {
            var command = new UpdateAdminUserCommand(
                userId,
                request.Username,
                request.DisplayName,
                request.Email,
                request.Bio,
                request.AvatarUrl
            );
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPost("{userId:guid}/roles")]
        public async Task<ActionResult<Result<bool>>> UpdateUserRole(
            Guid userId,
            [FromBody] UpdateUserRoleRequest request,
            CancellationToken ct = default)
        {
            var command = new UpdateUserRoleCommand(userId, request.Role);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPost("{userId:guid}/ban")]
        public async Task<ActionResult<Result<bool>>> BanUser(
            Guid userId,
            CancellationToken ct = default)
        {
            var command = new BanUserCommand(userId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        [HttpPost("{userId:guid}/unban")]
        public async Task<ActionResult<Result<bool>>> UnbanUser(
            Guid userId,
            CancellationToken ct = default)
        {
            var command = new UnbanUserCommand(userId);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
