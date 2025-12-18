using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Predictions.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Predictions.Queries.GetUserPredictions
{
    public sealed class GetUserPredictionsQueryHandler : IRequestHandler<GetUserPredictionsQuery, Result<List<PredictionDto>>>
    {
        private readonly ITipprDbContext _db;
        private readonly ICurrentUser _currentUser;
        private readonly IMapper _mapper;

        public GetUserPredictionsQueryHandler(ITipprDbContext db, ICurrentUser currentUser, IMapper mapper)
        {
            _db = db;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<List<PredictionDto>>> Handle(GetUserPredictionsQuery request, CancellationToken ct)
        {
            var userId = _currentUser.UserId;

            var items = await _db.Predictions
                .AsNoTracking()
                .Where(p => p.UserId == userId && p.LeagueId == request.LeagueId)
                .OrderByDescending(p => p.UpdatedAt)
                .ProjectTo<PredictionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Result<List<PredictionDto>>.Success(items);
        }
    }

}
