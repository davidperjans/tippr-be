using FluentValidation;

namespace Application.Features.Admin.Leagues.Commands.UpdateAdminLeague
{
    public class UpdateAdminLeagueCommandValidator : AbstractValidator<UpdateAdminLeagueCommand>
    {
        public UpdateAdminLeagueCommandValidator()
        {
            RuleFor(x => x.LeagueId)
                .NotEmpty()
                .WithMessage("LeagueId is required");

            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage("Name cannot exceed 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

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
