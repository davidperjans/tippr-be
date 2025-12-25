using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Venues.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Venues.Queries.GetVenueByMatch
{
    public sealed class GetVenueByMatchQueryHandler : IRequestHandler<GetVenueByMatchQuery, Result<VenueDto>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;

        public GetVenueByMatchQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<VenueDto>> Handle(GetVenueByMatchQuery request, CancellationToken ct)
        {
            var match = await _db.Matches
                .AsNoTracking()
                .Include(m => m.Venue)
                .SingleOrDefaultAsync(m => m.Id == request.MatchId, ct);

            if (match == null)
                return Result<VenueDto>.NotFound("Match not found", "match.not_found");

            if (match.Venue == null)
            {
                // Return a minimal venue DTO from match's inline venue info if available
                if (!string.IsNullOrEmpty(match.VenueName))
                {
                    return Result<VenueDto>.Success(new VenueDto
                    {
                        Id = Guid.Empty,
                        Name = match.VenueName,
                        City = match.VenueCity
                    });
                }

                return Result<VenueDto>.NotFound("Match has no venue information", "venue.not_found");
            }

            var venueDto = _mapper.Map<VenueDto>(match.Venue);
            return Result<VenueDto>.Success(venueDto);
        }
    }
}
