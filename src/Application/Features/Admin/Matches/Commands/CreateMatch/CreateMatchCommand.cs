using Application.Common;
using Domain.Enums;
using MediatR;

namespace Application.Features.Admin.Matches.Commands.CreateMatch
{
    public sealed record CreateMatchCommand(
        Guid TournamentId,
        Guid HomeTeamId,
        Guid AwayTeamId,
        DateTime MatchDate,
        MatchStage Stage,
        string? Venue,
        int? ApiFootballId
    ) : IRequest<Result<Guid>>;
}
