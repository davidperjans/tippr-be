using FluentValidation;

namespace Application.Features.Leagues.Commands.JoinLeague
{
    public class JoinLeagueCommandValidator : AbstractValidator<JoinLeagueCommand>
    {
        public JoinLeagueCommandValidator()
        {
            RuleFor(x => x.LeagueId).NotEmpty();

            RuleFor(x => x.InviteCode)
                .MaximumLength(20)
                .When(x => x.InviteCode != null);
        }
    }
}
