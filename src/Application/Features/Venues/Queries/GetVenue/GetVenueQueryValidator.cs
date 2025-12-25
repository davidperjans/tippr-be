using FluentValidation;

namespace Application.Features.Venues.Queries.GetVenue
{
    public sealed class GetVenueQueryValidator : AbstractValidator<GetVenueQuery>
    {
        public GetVenueQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
