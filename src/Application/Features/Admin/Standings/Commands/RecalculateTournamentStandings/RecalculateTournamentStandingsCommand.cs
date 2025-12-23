using Application.Common;
using MediatR;

namespace Application.Features.Admin.Standings.Commands.RecalculateTournamentStandings
{
    public sealed record RecalculateTournamentStandingsCommand(Guid TournamentId) : IRequest<Result<RecalculateTournamentStandingsResult>>;

    public class RecalculateTournamentStandingsResult
    {
        public int LeaguesUpdated { get; init; }
        public int TotalMembersUpdated { get; init; }
    }
}
