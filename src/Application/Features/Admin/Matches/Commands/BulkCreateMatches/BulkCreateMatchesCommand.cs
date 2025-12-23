using Application.Common;
using Domain.Enums;
using MediatR;

namespace Application.Features.Admin.Matches.Commands.BulkCreateMatches
{
    public sealed record BulkCreateMatchesCommand(
        Guid TournamentId,
        List<BulkMatchItem> Matches
    ) : IRequest<Result<BulkCreateMatchesResult>>;

    public sealed record BulkMatchItem(
        Guid HomeTeamId,
        Guid AwayTeamId,
        DateTime MatchDate,
        MatchStage Stage,
        string? Venue,
        int? ApiFootballId
    );

    public class BulkCreateMatchesResult
    {
        public int CreatedCount { get; init; }
        public int FailedCount { get; init; }
        public List<string> Errors { get; init; } = new();
        public List<Guid> CreatedIds { get; init; } = new();
    }
}
