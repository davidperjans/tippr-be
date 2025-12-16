using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Tournaments.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tournaments.Queries.GetAllTournaments
{
    public class GetAllTournamentsQueryHandler : IRequestHandler<GetAllTournamentsQuery, Result<IReadOnlyList<TournamentDto>>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;
        public GetAllTournamentsQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IReadOnlyList<TournamentDto>>> Handle(GetAllTournamentsQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Tournaments.AsNoTracking();

            if (request.OnlyActive)
                query = query.Where(x => x.IsActive);

            var list = await query
                .OrderByDescending(x => x.Year)
                .ProjectTo<TournamentDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<TournamentDto>>.Success(list);
        }
    }
}
