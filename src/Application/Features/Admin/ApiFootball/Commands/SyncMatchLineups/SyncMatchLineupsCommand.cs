using Application.Common;
using MediatR;

namespace Application.Features.Admin.ApiFootball.Commands.SyncMatchLineups
{
    public sealed record SyncMatchLineupsCommand(
        Guid MatchId,
        bool Force = false
    ) : IRequest<Result<SyncMatchLineupsResult>>;

    public sealed class SyncMatchLineupsResult
    {
        public bool Success { get; init; }
        public bool LineupsAvailable { get; init; }
        public int TeamsWithLineups { get; init; }
        public DateTime? FetchedAt { get; init; }
        public string? Message { get; init; }
    }
}
