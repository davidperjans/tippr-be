using Application.Common;
using Application.Features.Admin.DTOs;
using MediatR;

namespace Application.Features.Admin.Leagues.Queries.GetAdminLeagueMembers
{
    public sealed record GetAdminLeagueMembersQuery(Guid LeagueId) : IRequest<Result<IReadOnlyList<AdminLeagueMemberDto>>>;
}
