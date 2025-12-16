using FluentValidation;

namespace Application.Features.Tournaments.Commands.CreateTournament
{
    public class CreateTournamentCommandValidator : AbstractValidator<CreateTournamentCommand>
    {
        public CreateTournamentCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Year)
                .GreaterThan(1900);

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate);
        }
    }
}
