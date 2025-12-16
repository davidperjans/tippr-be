using Application.Common;
using Application.Features.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Tournaments.Queries.GetTournamentById
{
    public sealed record GetTournamentByIdQuery(
        Guid Id
    ) : IRequest<Result<TournamentDto>>;
}
