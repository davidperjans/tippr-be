using FluentValidation;

namespace Application.Features.Leagues.Commands.CreateLeague
{
    public class CreateLeagueCommandValidator : AbstractValidator<CreateLeagueCommand>
    {
        public CreateLeagueCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("League name is required")
                .MaximumLength(100)
                .WithMessage("League name cannot exceed 100 characters");

            RuleFor(x => x.TournamentId)
                .NotEmpty()
                .WithMessage("Tournament is required");

            RuleFor(x => x.OwnerId)
                .NotEmpty()
                .WithMessage("Owner is required");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters")
                .When(x => x.Description != null);

            RuleFor(x => x.MaxMembers)
                .GreaterThan(0)
                .WithMessage("MaxMembers must be greater than 0")
                .LessThanOrEqualTo(1000)
                .WithMessage("MaxMembers cannot exceed 1000")
                .When(x => x.MaxMembers.HasValue);

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500)
                .WithMessage("ImageUrl cannot exceed 500 characters")
                .When(x => x.ImageUrl != null);
        }
    }
}
