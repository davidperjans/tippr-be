using Application.Common;
using Application.Features.Venues.DTOs;
using MediatR;

namespace Application.Features.Venues.Queries.GetVenue
{
    public sealed record GetVenueQuery(Guid Id) : IRequest<Result<VenueDto>>;
}
