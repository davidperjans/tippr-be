using FluentValidation;

namespace Application.Features.Venues.Queries.GetVenueByMatch
{
    public sealed class GetVenueByMatchQueryValidator : AbstractValidator<GetVenueByMatchQuery>
    {
        public GetVenueByMatchQueryValidator()
        {
            RuleFor(x => x.MatchId).NotEmpty();
        }
    }
}
