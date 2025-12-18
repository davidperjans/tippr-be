using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Matches.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Matches.Queries.GetMatchesByDate
{
    public sealed class GetMatchesByDateQueryHandler : IRequestHandler<GetMatchesByDateQuery, Result<IReadOnlyList<MatchDto>>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;

        public GetMatchesByDateQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IReadOnlyList<MatchDto>>> Handle(GetMatchesByDateQuery request, CancellationToken ct)
        {
            var startUtc = request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endUtc = startUtc.AddDays(1);

            var matches = await _db.Matches
                .AsNoTracking()
                .Where(m => m.MatchDate >= startUtc && m.MatchDate < endUtc)
                .OrderBy(m => m.MatchDate)
                .ProjectTo<MatchDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Result<IReadOnlyList<MatchDto>>.Success(matches);
        }
    }
}
