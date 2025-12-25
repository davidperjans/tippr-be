using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Venues.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Venues.Queries.GetVenueByTeam
{
    public sealed class GetVenueByTeamQueryHandler : IRequestHandler<GetVenueByTeamQuery, Result<VenueDto>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;

        public GetVenueByTeamQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<VenueDto>> Handle(GetVenueByTeamQuery request, CancellationToken ct)
        {
            var team = await _db.Teams
                .AsNoTracking()
                .Include(t => t.Venue)
                .SingleOrDefaultAsync(t => t.Id == request.TeamId, ct);

            if (team == null)
                return Result<VenueDto>.NotFound("Team not found", "team.not_found");

            if (team.Venue == null)
                return Result<VenueDto>.NotFound("Team has no home venue", "venue.not_found");

            var venueDto = _mapper.Map<VenueDto>(team.Venue);
            return Result<VenueDto>.Success(venueDto);
        }
    }
}
