using Application.Common;
using Application.Features.Leagues.DTOs;
using Domain.Enums;
using MediatR;

namespace Application.Features.Leagues.Commands.UpdateLeagueSettings
{
    public sealed record UpdateLeagueSettingsCommand(
        Guid LeagueId,
        Guid UserId,
        PredictionMode PredictionMode,
        int DeadlineMinutes,
        int PointsCorrectScore,
        int PointsCorrectOutcome,
        int PointsCorrectGoals,
        int PointsRoundOf16Team,
        int PointsQuarterFinalTeam,
        int PointsSemiFinalTeam,
        int PointsFinalTeam,
        int PointsTopScorer,
        int PointsWinner,
        int PointsMostGoalsGroup,
        int PointsMostConcededGroup,
        bool AllowLateEdits
    ) : IRequest<Result<LeagueSettingsDto>>;
}
