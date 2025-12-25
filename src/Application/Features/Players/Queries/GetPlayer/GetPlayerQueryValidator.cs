using FluentValidation;

namespace Application.Features.Players.Queries.GetPlayer
{
    public sealed class GetPlayerQueryValidator : AbstractValidator<GetPlayerQuery>
    {
        public GetPlayerQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
