using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Venues.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Venues.Queries.GetVenues
{
    public sealed class GetVenuesQueryHandler : IRequestHandler<GetVenuesQuery, Result<IReadOnlyList<VenueDto>>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;

        public GetVenuesQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IReadOnlyList<VenueDto>>> Handle(GetVenuesQuery request, CancellationToken ct)
        {
            IQueryable<Domain.Entities.Venue> query = _db.Venues.AsNoTracking();

            if (request.TournamentId.HasValue)
            {
                // Get venues used in matches for this tournament
                var venueIdsInMatches = await _db.Matches
                    .Where(m => m.TournamentId == request.TournamentId.Value && m.VenueId.HasValue)
                    .Select(m => m.VenueId!.Value)
                    .Distinct()
                    .ToListAsync(ct);

                // Also get venues from teams in this tournament
                var venueIdsFromTeams = await _db.Teams
                    .Where(t => t.TournamentId == request.TournamentId.Value && t.VenueId.HasValue)
                    .Select(t => t.VenueId!.Value)
                    .Distinct()
                    .ToListAsync(ct);

                var allVenueIds = venueIdsInMatches.Union(venueIdsFromTeams).ToList();
                query = query.Where(v => allVenueIds.Contains(v.Id));
            }

            var venues = await query
                .OrderBy(v => v.Name)
                .ProjectTo<VenueDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Result<IReadOnlyList<VenueDto>>.Success(venues);
        }
    }
}
