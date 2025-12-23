using Application.Common;
using MediatR;

namespace Application.Features.Admin.Teams.Commands.CreateTeam
{
    public sealed record CreateTeamCommand(
        Guid TournamentId,
        string Name,
        string Code,
        string? FlagUrl,
        string? GroupName,
        int? FifaRank,
        decimal? FifaPoints,
        int? ApiFootballId
    ) : IRequest<Result<Guid>>;
}
