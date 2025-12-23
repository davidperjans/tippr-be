using FluentValidation;

namespace Application.Features.Admin.Tournaments.Commands.UpdateTournament
{
    public class UpdateTournamentCommandValidator : AbstractValidator<UpdateTournamentCommand>
    {
        public UpdateTournamentCommandValidator()
        {
            RuleFor(x => x.TournamentId)
                .NotEmpty()
                .WithMessage("TournamentId is required");

            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage("Name cannot exceed 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            RuleFor(x => x.Year)
                .GreaterThan(1900)
                .WithMessage("Year must be greater than 1900")
                .When(x => x.Year.HasValue);

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("EndDate must be after StartDate")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

            RuleFor(x => x.LogoUrl)
                .MaximumLength(500)
                .WithMessage("LogoUrl cannot exceed 500 characters")
                .When(x => x.LogoUrl != null);
        }
    }
}
