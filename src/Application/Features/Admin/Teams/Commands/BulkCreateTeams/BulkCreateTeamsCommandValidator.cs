using FluentValidation;

namespace Application.Features.Admin.Teams.Commands.BulkCreateTeams
{
    public class BulkCreateTeamsCommandValidator : AbstractValidator<BulkCreateTeamsCommand>
    {
        public BulkCreateTeamsCommandValidator()
        {
            RuleFor(x => x.TournamentId)
                .NotEmpty()
                .WithMessage("TournamentId is required");

            RuleFor(x => x.Teams)
                .NotEmpty()
                .WithMessage("Teams list cannot be empty");

            RuleForEach(x => x.Teams).ChildRules(team =>
            {
                team.RuleFor(t => t.Name)
                    .NotEmpty()
                    .WithMessage("Team name is required")
                    .MaximumLength(100)
                    .WithMessage("Team name cannot exceed 100 characters");

                team.RuleFor(t => t.Code)
                    .NotEmpty()
                    .WithMessage("Team code is required")
                    .MaximumLength(10)
                    .WithMessage("Team code cannot exceed 10 characters");
            });
        }
    }
}
