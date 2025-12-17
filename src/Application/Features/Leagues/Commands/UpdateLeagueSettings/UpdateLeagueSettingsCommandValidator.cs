using FluentValidation;

namespace Application.Features.Leagues.Commands.UpdateLeagueSettings
{
    public sealed class UpdateLeagueSettingsCommandValidator : AbstractValidator<UpdateLeagueSettingsCommand>
    {
        public UpdateLeagueSettingsCommandValidator()
        {
            RuleFor(x => x.LeagueId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();

            RuleFor(x => x.DeadlineMinutes)
                .InclusiveBetween(0, 24 * 60);

            // Points: rimliga ranges (du kan justera senare)
            RuleFor(x => x.PointsCorrectScore).InclusiveBetween(0, 100);
            RuleFor(x => x.PointsCorrectOutcome).InclusiveBetween(0, 100);
            RuleFor(x => x.PointsCorrectGoals).InclusiveBetween(0, 100);

            RuleFor(x => x.PointsRoundOf16Team).InclusiveBetween(0, 100);
            RuleFor(x => x.PointsQuarterFinalTeam).InclusiveBetween(0, 100);
            RuleFor(x => x.PointsSemiFinalTeam).InclusiveBetween(0, 100);
            RuleFor(x => x.PointsFinalTeam).InclusiveBetween(0, 100);

            RuleFor(x => x.PointsTopScorer).InclusiveBetween(0, 200);
            RuleFor(x => x.PointsWinner).InclusiveBetween(0, 200);
            RuleFor(x => x.PointsMostGoalsGroup).InclusiveBetween(0, 200);
            RuleFor(x => x.PointsMostConcededGroup).InclusiveBetween(0, 200);
        }
    }
}
