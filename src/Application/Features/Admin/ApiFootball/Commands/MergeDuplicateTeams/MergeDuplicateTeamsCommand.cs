using Application.Common;
using MediatR;

namespace Application.Features.Admin.ApiFootball.Commands.MergeDuplicateTeams
{
    public sealed record MergeDuplicateTeamsCommand(
        Guid TournamentId,
        bool DryRun = true  // Default to dry run for safety
    ) : IRequest<Result<MergeDuplicateTeamsResult>>;

    public sealed record MergeDuplicateTeamsResult
    {
        public int TeamsMerged { get; init; }
        public int TeamsDeleted { get; init; }
        public int MatchesUpdated { get; init; }
        public int PredictionsUpdated { get; init; }
        public int FavoritesUpdated { get; init; }
        public bool WasDryRun { get; init; }
        public List<MergeAction> MergeActions { get; init; } = new();
    }

    public sealed record MergeAction
    {
        public string OldTeamName { get; init; } = string.Empty;
        public Guid OldTeamId { get; init; }
        public string NewTeamName { get; init; } = string.Empty;
        public Guid NewTeamId { get; init; }
        public string TransferredDisplayName { get; init; } = string.Empty;
        public int? TransferredFifaRank { get; init; }
        public decimal? TransferredFifaPoints { get; init; }
    }
}
