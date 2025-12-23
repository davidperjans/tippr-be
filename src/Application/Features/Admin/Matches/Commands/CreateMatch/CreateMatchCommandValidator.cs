using FluentValidation;

namespace Application.Features.Admin.Matches.Commands.CreateMatch
{
    public class CreateMatchCommandValidator : AbstractValidator<CreateMatchCommand>
    {
        public CreateMatchCommandValidator()
        {
            RuleFor(x => x.TournamentId)
                .NotEmpty()
                .WithMessage("TournamentId is required");

            RuleFor(x => x.HomeTeamId)
                .NotEmpty()
                .WithMessage("HomeTeamId is required");

            RuleFor(x => x.AwayTeamId)
                .NotEmpty()
                .WithMessage("AwayTeamId is required");

            RuleFor(x => x.MatchDate)
                .NotEmpty()
                .WithMessage("MatchDate is required");

            RuleFor(x => x.Stage)
                .IsInEnum()
                .WithMessage("Invalid match stage");

            RuleFor(x => x.Venue)
                .MaximumLength(200)
                .WithMessage("Venue cannot exceed 200 characters")
                .When(x => x.Venue != null);
        }
    }
}
