using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Predictions.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.BonusQuestions.Queries.GetBonusQuestionsByTournament
{
    public sealed class GetBonusQuestionsByTournamentQueryHandler : IRequestHandler<GetBonusQuestionsByTournamentQuery, Result<IReadOnlyList<BonusQuestionDto>>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;

        public GetBonusQuestionsByTournamentQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IReadOnlyList<BonusQuestionDto>>> Handle(GetBonusQuestionsByTournamentQuery request, CancellationToken ct)
        {
            var bonusQuestions = await _db.BonusQuestions
                .AsNoTracking()
                .Where(bq => bq.TournamentId == request.TournamentId)
                .OrderBy(bq => bq.QuestionType)
                .ProjectTo<BonusQuestionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Result<IReadOnlyList<BonusQuestionDto>>.Success(bonusQuestions);
        }
    }
}
