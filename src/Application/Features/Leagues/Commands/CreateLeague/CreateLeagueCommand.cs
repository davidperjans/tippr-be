using Application.Common;
using MediatR;

namespace Application.Features.Leagues.Commands.CreateLeague
{
    public sealed record CreateLeagueCommand(
        string Name,
        string? Description,
        Guid TournamentId,
        bool IsPublic,
        int? MaxMembers,
        string? ImageUrl
    ) : IRequest<Result<Guid>>;
}
