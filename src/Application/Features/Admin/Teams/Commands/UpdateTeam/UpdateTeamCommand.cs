using Application.Common;
using Application.Features.Admin.DTOs;
using MediatR;

namespace Application.Features.Admin.Teams.Commands.UpdateTeam
{
    public sealed record UpdateTeamCommand(
        Guid TeamId,
        string? Name,
        string? Code,
        string? FlagUrl,
        string? GroupName,
        int? FifaRank,
        decimal? FifaPoints,
        int? ApiFootballId
    ) : IRequest<Result<AdminTeamDto>>;
}
