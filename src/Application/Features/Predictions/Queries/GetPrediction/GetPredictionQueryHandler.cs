using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Predictions.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Predictions.Queries.GetPrediction
{
    public sealed class GetPredictionQueryHandler : IRequestHandler<GetPredictionQuery, Result<PredictionDto?>>
    {
        private readonly ITipprDbContext _db;
        private readonly ICurrentUser _currentUser;
        private readonly IMapper _mapper;

        public GetPredictionQueryHandler(ITipprDbContext db, ICurrentUser currentUser, IMapper mapper)
        {
            _db = db;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<Result<PredictionDto?>> Handle(GetPredictionQuery request, CancellationToken ct)
        {
            var userId = _currentUser.UserId;

            var entity = await _db.Predictions
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.UserId == userId &&
                    p.LeagueId == request.LeagueId &&
                    p.MatchId == request.MatchId, ct);

            if (entity == null)
                return Result<PredictionDto?>.NotFound("no prediction found", "prediction.not_found");

            var dto = _mapper.Map<PredictionDto>(entity);

            return Result<PredictionDto?>.Success(dto);
        }
    }

}
