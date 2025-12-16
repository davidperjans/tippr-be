using Application.Common;
using Application.Features.Tournaments.DTOs;
using Domain.Enums;
using MediatR;

namespace Application.Features.Tournaments.Commands.CreateTournament
{
    public sealed record CreateTournamentCommand(
        string Name,
        int Year,
        TournamentType Type,
        DateTime StartDate,
        DateTime EndDate,
        string Country,
        string? LogoUrl
    ) : IRequest<Result<Guid>>;
}
