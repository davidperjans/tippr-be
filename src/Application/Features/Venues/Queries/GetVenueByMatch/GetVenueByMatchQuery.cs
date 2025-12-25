using Application.Common;
using Application.Features.Venues.DTOs;
using MediatR;

namespace Application.Features.Venues.Queries.GetVenueByMatch
{
    public sealed record GetVenueByMatchQuery(Guid MatchId) : IRequest<Result<VenueDto>>;
}
