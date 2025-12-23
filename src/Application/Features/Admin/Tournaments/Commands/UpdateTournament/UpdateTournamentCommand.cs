using Application.Common;
using Application.Features.Admin.DTOs;
using Domain.Enums;
using MediatR;

namespace Application.Features.Admin.Tournaments.Commands.UpdateTournament
{
    public sealed record UpdateTournamentCommand(
        Guid TournamentId,
        string? Name,
        int? Year,
        TournamentType? Type,
        DateTime? StartDate,
        DateTime? EndDate,
        string? LogoUrl
    ) : IRequest<Result<AdminTournamentDto>>;
}
