using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Matches.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Matches.Queries.GetMatch
{
    public sealed class GetMatchQueryHandler : IRequestHandler<GetMatchQuery, Result<MatchDto>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;

        public GetMatchQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<MatchDto>> Handle(GetMatchQuery request, CancellationToken ct)
        {
            var match = await _db.Matches
                .AsNoTracking()
                .Where(m => m.Id == request.Id)
                .ProjectTo<MatchDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);

            if (match == null)
                return Result<MatchDto>.NotFound("match not found", "match.not_found");

            return Result<MatchDto>.Success(match);
        }
    }
}
