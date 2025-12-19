using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Predictions.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.BonusQuestions.Queries.GetUserBonusPredictions
{
    public sealed class GetUserBonusPredictionsQueryHandler : IRequestHandler<GetUserBonusPredictionsQuery, Result<List<BonusPredictionDto>>>
    {
        private readonly ITipprDbContext _db;
        private readonly ICurrentUser _currentUser;
        private readonly IMapper _mapper;

        public GetUserBonusPredictionsQueryHandler(ITipprDbContext db, ICurrentUser currentUser, IMapper mapper)
        {
            _db = db;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<List<BonusPredictionDto>>> Handle(GetUserBonusPredictionsQuery request, CancellationToken ct)
        {
            var userId = _currentUser.UserId;

            var items = await _db.BonusPredictions
                .AsNoTracking()
                .Where(bp => bp.UserId == userId && bp.LeagueId == request.LeagueId)
                .OrderBy(bp => bp.BonusQuestionId)
                .ProjectTo<BonusPredictionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Result<List<BonusPredictionDto>>.Success(items);
        }
    }
}
