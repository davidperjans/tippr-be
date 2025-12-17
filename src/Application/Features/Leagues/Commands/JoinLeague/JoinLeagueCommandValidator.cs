using FluentValidation;

namespace Application.Features.Leagues.Commands.JoinLeague
{
    public class JoinLeagueCommandValidator : AbstractValidator<JoinLeagueCommand>
    {
        public JoinLeagueCommandValidator()
        {
            RuleFor(x => x.LeagueId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();

            RuleFor(x => x.InviteCode)
                .NotEmpty()
                .MaximumLength(20);
        }
    }
}
