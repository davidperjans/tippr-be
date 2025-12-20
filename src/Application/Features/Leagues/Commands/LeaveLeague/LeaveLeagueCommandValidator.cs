using FluentValidation;

namespace Application.Features.Leagues.Commands.LeaveLeague
{
    public sealed class LeaveLeagueCommandValidator : AbstractValidator<LeaveLeagueCommand>
    {
        public LeaveLeagueCommandValidator()
        {
            RuleFor(x => x.LeagueId).NotEmpty();
        }
    }
}
