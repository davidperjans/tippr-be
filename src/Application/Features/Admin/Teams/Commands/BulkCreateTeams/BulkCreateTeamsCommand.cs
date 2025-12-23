using Application.Common;
using MediatR;

namespace Application.Features.Admin.Teams.Commands.BulkCreateTeams
{
    public sealed record BulkCreateTeamsCommand(
        Guid TournamentId,
        List<BulkTeamItem> Teams
    ) : IRequest<Result<BulkCreateTeamsResult>>;

    public sealed record BulkTeamItem(
        string Name,
        string Code,
        string? FlagUrl,
        string? GroupName,
        int? FifaRank,
        decimal? FifaPoints,
        int? ApiFootballId
    );

    public class BulkCreateTeamsResult
    {
        public int CreatedCount { get; init; }
        public int SkippedCount { get; init; }
        public List<string> SkippedTeams { get; init; } = new();
        public List<Guid> CreatedIds { get; init; } = new();
    }
}
