using Application.Common;
using Application.Features.Admin.DTOs;
using MediatR;

namespace Application.Features.Admin.Leagues.Commands.UpdateAdminLeague
{
    public sealed record UpdateAdminLeagueCommand(
        Guid LeagueId,
        string? Name,
        string? Description,
        bool? IsPublic,
        bool? IsGlobal,
        int? MaxMembers,
        string? ImageUrl
    ) : IRequest<Result<AdminLeagueDto>>;
}
