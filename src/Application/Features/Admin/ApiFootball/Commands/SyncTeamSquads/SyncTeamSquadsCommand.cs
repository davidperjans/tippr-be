using Application.Common;
using MediatR;

namespace Application.Features.Admin.ApiFootball.Commands.SyncTeamSquads
{
    public sealed record SyncTeamSquadsCommand(
        Guid TournamentId,
        bool Force = false
    ) : IRequest<Result<SyncTeamSquadsResult>>;

    public sealed record SyncTeamSquadsResult
    {
        public int TeamsProcessed { get; init; }
        public int TeamsSkipped { get; init; }
        public int PlayersCreated { get; init; }
        public int PlayersUpdated { get; init; }
        public List<string> Warnings { get; init; } = new();
        public DateTime SyncedAt { get; init; }
    }
}
