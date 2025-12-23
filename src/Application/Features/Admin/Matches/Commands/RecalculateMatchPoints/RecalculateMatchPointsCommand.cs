using Application.Common;
using MediatR;

namespace Application.Features.Admin.Matches.Commands.RecalculateMatchPoints
{
    public sealed record RecalculateMatchPointsCommand(Guid MatchId) : IRequest<Result<RecalculateMatchPointsResult>>;

    public class RecalculateMatchPointsResult
    {
        public int PredictionsUpdated { get; init; }
        public int LeaguesAffected { get; init; }
    }
}
