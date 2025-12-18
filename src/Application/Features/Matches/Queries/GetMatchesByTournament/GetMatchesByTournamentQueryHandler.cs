using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Matches.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Matches.Queries.GetMatchesByTournament
{
    public sealed class GetMatchesByTournamentQueryHandler : IRequestHandler<GetMatchesByTournamentQuery, Result<IReadOnlyList<MatchDto>>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;

        public GetMatchesByTournamentQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IReadOnlyList<MatchDto>>> Handle(GetMatchesByTournamentQuery request, CancellationToken ct)
        {
            var matches = await _db.Matches
                .AsNoTracking()
                .Where(m => m.TournamentId == request.TournamentId)
                .OrderBy(m => m.MatchDate)
                .ProjectTo<MatchDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Result<IReadOnlyList<MatchDto>>.Success(matches);
        }
    }
}
