using Application.Common;
using Application.Features.Venues.DTOs;
using MediatR;

namespace Application.Features.Venues.Queries.GetVenueByTeam
{
    public sealed record GetVenueByTeamQuery(Guid TeamId) : IRequest<Result<VenueDto>>;
}
