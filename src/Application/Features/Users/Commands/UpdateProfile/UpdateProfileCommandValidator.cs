using FluentValidation;

namespace Application.Features.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required");

            RuleFor(x => x.DisplayName)
                .MaximumLength(100)
                .WithMessage("DisplayName cannot exceed 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

            RuleFor(x => x.Bio)
                .MaximumLength(500)
                .WithMessage("Bio cannot exceed 500 characters")
                .When(x => x.Bio != null);
        }
    }
}
