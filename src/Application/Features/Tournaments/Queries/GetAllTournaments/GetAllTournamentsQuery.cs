using Application.Common;
using Application.Features.Tournaments.DTOs;
using MediatR;

namespace Application.Features.Tournaments.Queries.GetAllTournaments
{
    public record GetAllTournamentsQuery(bool OnlyActive = false) : IRequest<Result<IReadOnlyList<TournamentDto>>>;
}
