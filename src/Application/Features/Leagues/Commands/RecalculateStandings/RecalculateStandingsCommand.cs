using Application.Common;
using MediatR;

namespace Application.Features.Leagues.Commands.RecalculateStandings
{
    /// <summary>
    /// Command to trigger a full recalculation of standings for a league.
    /// Useful for data integrity checks or after bulk data imports.
    /// </summary>
    public sealed record RecalculateStandingsCommand(Guid LeagueId) : IRequest<Result<bool>>;
}
