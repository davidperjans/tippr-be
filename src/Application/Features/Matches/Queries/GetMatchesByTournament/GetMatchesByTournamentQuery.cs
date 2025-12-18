using Application.Common;
using Application.Features.Matches.DTOs;
using MediatR;

namespace Application.Features.Matches.Queries.GetMatchesByTournament
{
    public sealed record GetMatchesByTournamentQuery(
        Guid TournamentId
    ) : IRequest<Result<IReadOnlyList<MatchDto>>>;
}
