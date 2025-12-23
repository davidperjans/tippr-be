using Application.Common;
using MediatR;

namespace Application.Features.Admin.ApiFootball.Commands.SyncTournamentBaseline
{
    public sealed record SyncTournamentBaselineCommand(
        Guid TournamentId,
        bool Force = false,
        bool CreateMissingTeams = false
    ) : IRequest<Result<SyncTournamentBaselineResult>>;

    public sealed record SyncTournamentBaselineResult
    {
        public int TeamsUpdated { get; init; }
        public int TeamsCreated { get; init; }
        public int TeamsUnmapped { get; init; }
        public int VenuesUpserted { get; init; }
        public int MatchesUpserted { get; init; }
        public int MatchesLinked { get; init; }
        public int MatchesSkipped { get; init; }
        public List<string> UnmappedTeams { get; init; } = new();
        public List<string> Warnings { get; init; } = new();
        public DateTime SyncedAt { get; init; }
    }
}
