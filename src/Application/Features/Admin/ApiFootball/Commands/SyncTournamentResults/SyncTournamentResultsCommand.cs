using Application.Common;
using MediatR;

namespace Application.Features.Admin.ApiFootball.Commands.SyncTournamentResults
{
    public sealed record SyncTournamentResultsCommand(
        Guid TournamentId,
        bool Force = false
    ) : IRequest<Result<SyncTournamentResultsResult>>;

    public sealed class SyncTournamentResultsResult
    {
        public int MatchesUpdated { get; init; }
        public int MatchesUnchanged { get; init; }
        public int MatchesNotFound { get; init; }
        public int ApiCallsMade { get; init; }
        public List<MatchUpdateInfo> Updates { get; init; } = new();
        public List<string> Warnings { get; init; } = new();
        public DateTime SyncedAt { get; init; }
    }

    public sealed class MatchUpdateInfo
    {
        public Guid MatchId { get; init; }
        public string HomeTeam { get; init; } = string.Empty;
        public string AwayTeam { get; init; } = string.Empty;
        public string OldStatus { get; init; } = string.Empty;
        public string NewStatus { get; init; } = string.Empty;
        public string? OldScore { get; init; }
        public string? NewScore { get; init; }
    }
}
