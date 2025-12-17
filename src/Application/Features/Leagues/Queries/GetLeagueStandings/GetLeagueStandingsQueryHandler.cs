using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Leagues.Queries.GetLeagueStandings
{
    public class GetLeagueStandingsQueryHandler : IRequestHandler<GetLeagueStandingsQuery, Result<IReadOnlyList<LeagueStandingDto>>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;
        public GetLeagueStandingsQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<Result<IReadOnlyList<LeagueStandingDto>>> Handle(GetLeagueStandingsQuery request, CancellationToken cancellationToken)
        {
            var leagueExists = await _db.Leagues
                .AnyAsync(l => l.Id == request.LeagueId, cancellationToken);

            if (!leagueExists)
                return Result<IReadOnlyList<LeagueStandingDto>>.Failure("league not found.");

            var userIsMember = await _db.LeagueMembers
                .AnyAsync(m =>
                    m.LeagueId == request.LeagueId &&
                    m.UserId == request.UserId,
                    cancellationToken);

            if (!userIsMember)
                return Result<IReadOnlyList<LeagueStandingDto>>.Failure("not a member of this league.");

            var standings = await _db.LeagueStandings
                .AsNoTracking()
                .Where(s => s.LeagueId == request.LeagueId)
                .OrderBy(s => s.Rank)
                .ProjectTo<LeagueStandingDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<LeagueStandingDto>>.Success(standings);
        }
    }
}
