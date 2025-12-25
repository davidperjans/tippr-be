using Application.Common;
using MediatR;

namespace Application.Features.Admin.ApiFootball.Commands.SyncGroupStandings
{
    public sealed record SyncGroupStandingsCommand(
        Guid TournamentId,
        bool Force = false
    ) : IRequest<Result<SyncGroupStandingsResult>>;

    public sealed record SyncGroupStandingsResult
    {
        public int GroupsCreated { get; init; }
        public int GroupsUpdated { get; init; }
        public int TeamsAssignedToGroups { get; init; }
        public int StandingsUpserted { get; init; }
        public List<string> Warnings { get; init; } = new();
        public DateTime SyncedAt { get; init; }
    }
}
