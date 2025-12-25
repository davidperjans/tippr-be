using FluentValidation;

namespace Application.Features.Venues.Queries.GetVenueByTeam
{
    public sealed class GetVenueByTeamQueryValidator : AbstractValidator<GetVenueByTeamQuery>
    {
        public GetVenueByTeamQueryValidator()
        {
            RuleFor(x => x.TeamId).NotEmpty();
        }
    }
}
