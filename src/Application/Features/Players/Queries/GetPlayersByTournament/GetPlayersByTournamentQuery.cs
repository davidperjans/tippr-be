using Application.Common;
using Application.Features.Players.DTOs;
using MediatR;

namespace Application.Features.Players.Queries.GetPlayersByTournament
{
    public sealed record GetPlayersByTournamentQuery(
        Guid TournamentId,
        string? Position = null,
        string? SearchTerm = null
    ) : IRequest<Result<IReadOnlyList<PlayerWithTeamDto>>>;
}
