using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.DTOs;
using Application.Features.Tournaments.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Leagues.Queries.GetLeague
{
    public class GetLeagueQueryHandler : IRequestHandler<GetLeagueQuery, Result<LeagueDto>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;
        public GetLeagueQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<Result<LeagueDto>> Handle(GetLeagueQuery request, CancellationToken cancellationToken)
        {
            var dto = await _db.Leagues
                .AsNoTracking()
                .Where(x => x.Id == request.LeagueId)
                .ProjectTo<LeagueDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (dto == null)
                return Result<LeagueDto>.NotFound("league not found", "league.not_found");

            return Result<LeagueDto>.Success(dto);
        }
    }
}
