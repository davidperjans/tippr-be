using FluentValidation;

namespace Application.Features.Admin.Teams.Commands.CreateTeam
{
    public class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
    {
        public CreateTeamCommandValidator()
        {
            RuleFor(x => x.TournamentId)
                .NotEmpty()
                .WithMessage("TournamentId is required");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required")
                .MaximumLength(100)
                .WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Code is required")
                .MaximumLength(10)
                .WithMessage("Code cannot exceed 10 characters");

            RuleFor(x => x.FlagUrl)
                .MaximumLength(500)
                .WithMessage("FlagUrl cannot exceed 500 characters")
                .When(x => x.FlagUrl != null);

            RuleFor(x => x.GroupName)
                .MaximumLength(50)
                .WithMessage("GroupName cannot exceed 50 characters")
                .When(x => x.GroupName != null);
        }
    }
}
