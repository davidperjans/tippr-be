using FluentValidation;

namespace Application.Features.Users.Commands.UploadAvatar
{
    public sealed class UploadAvatarCommandValidator : AbstractValidator<UploadAvatarCommand>
    {
        public UploadAvatarCommandValidator()
        {
            RuleFor(x => x.File).NotNull();
            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .LessThanOrEqualTo(2_000_000);

            RuleFor(x => x.File.ContentType)
                .Must(ct => ct is "image/jpeg" or "image/png" or "image/webp")
                .WithMessage("Only jpeg/png/webp allowed.");
        }
    }
}
