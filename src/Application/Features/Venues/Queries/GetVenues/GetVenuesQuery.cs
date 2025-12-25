using Application.Common;
using Application.Features.Venues.DTOs;
using MediatR;

namespace Application.Features.Venues.Queries.GetVenues
{
    public sealed record GetVenuesQuery(Guid? TournamentId = null) : IRequest<Result<IReadOnlyList<VenueDto>>>;
}
