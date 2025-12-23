using FluentValidation;

namespace Application.Features.Admin.Users.Commands.UpdateAdminUser
{
    public class UpdateAdminUserCommandValidator : AbstractValidator<UpdateAdminUserCommand>
    {
        public UpdateAdminUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required");

            RuleFor(x => x.Username)
                .MaximumLength(50)
                .WithMessage("Username cannot exceed 50 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Username));

            RuleFor(x => x.DisplayName)
                .MaximumLength(100)
                .WithMessage("DisplayName cannot exceed 100 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Invalid email format")
                .MaximumLength(255)
                .WithMessage("Email cannot exceed 255 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Bio)
                .MaximumLength(500)
                .WithMessage("Bio cannot exceed 500 characters")
                .When(x => x.Bio != null);

            RuleFor(x => x.AvatarUrl)
                .MaximumLength(500)
                .WithMessage("AvatarUrl cannot exceed 500 characters")
                .When(x => x.AvatarUrl != null);
        }
    }
}
