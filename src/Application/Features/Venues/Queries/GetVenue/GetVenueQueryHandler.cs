using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Venues.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Venues.Queries.GetVenue
{
    public sealed class GetVenueQueryHandler : IRequestHandler<GetVenueQuery, Result<VenueDto>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;

        public GetVenueQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<VenueDto>> Handle(GetVenueQuery request, CancellationToken ct)
        {
            var venue = await _db.Venues
                .AsNoTracking()
                .Where(v => v.Id == request.Id)
                .ProjectTo<VenueDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(ct);

            if (venue == null)
                return Result<VenueDto>.NotFound("Venue not found", "venue.not_found");

            return Result<VenueDto>.Success(venue);
        }
    }
}
