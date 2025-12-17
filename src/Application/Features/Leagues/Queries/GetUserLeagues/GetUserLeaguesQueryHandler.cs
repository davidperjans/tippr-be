using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.DTOs;
using Application.Features.Tournaments.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Leagues.Queries.GetUserLeagues
{
    public class GetUserLeaguesQueryHandler : IRequestHandler<GetUserLeaguesQuery, Result<IReadOnlyList<LeagueDto>>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;
        public GetUserLeaguesQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<Result<IReadOnlyList<LeagueDto>>> Handle(GetUserLeaguesQuery request, CancellationToken cancellationToken)
        {
            var list = await _db.LeagueMembers
                .AsNoTracking()
                .Where(lm => lm.UserId == request.UserId)
                .OrderByDescending(lm => lm.JoinedAt)
                .Select(lm => lm.League)
                .ProjectTo<LeagueDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<LeagueDto>>.Success(list);
        }
    }
}
