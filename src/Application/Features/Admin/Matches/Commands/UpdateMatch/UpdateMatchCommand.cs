using Application.Common;
using Application.Features.Admin.DTOs;
using Domain.Enums;
using MediatR;

namespace Application.Features.Admin.Matches.Commands.UpdateMatch
{
    public sealed record UpdateMatchCommand(
        Guid MatchId,
        DateTime? MatchDate,
        MatchStage? Stage,
        MatchStatus? Status,
        string? Venue,
        int? ApiFootballId
    ) : IRequest<Result<AdminMatchDto>>;
}
