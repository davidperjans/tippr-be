using Application.Common;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Matches.Commands.UpdateMatchResult
{
    public sealed class UpdateMatchResultCommandHandler : IRequestHandler<UpdateMatchResultCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;
        private readonly IStandingsService _standingsService;

        public UpdateMatchResultCommandHandler(ITipprDbContext db, IStandingsService standingsService)
        {
            _db = db;
            _standingsService = standingsService;
        }

        public async Task<Result<bool>> Handle(UpdateMatchResultCommand request, CancellationToken ct)
        {
            var match = await _db.Matches.FirstOrDefaultAsync(m => m.Id == request.MatchId, ct);

            if (match is null)
                return Result<bool>.NotFound("match not found.", "match.not_found");

            var willBeFinishedNow = IsFinished(request.Status);

            match.HomeScore = request.HomeScore;
            match.AwayScore = request.AwayScore;
            match.Status = request.Status;
            match.UpdatedAt = DateTime.UtcNow;

            if (!willBeFinishedNow)
            {
                await _db.SaveChangesAsync(ct);
                return Result<bool>.Success(true);
            }

            match.ResultVersion++;

            // 1) Spara matchresultatet
            await _db.SaveChangesAsync(ct);

            // 2) Scora predictions + uppdatera standings (förutsatt att den metoden använder samma db-context)
            await _standingsService.ScorePredictionsForMatchAsync(match.Id, match.ResultVersion, ct);

            // 3) Spara allt som standingsService ändrat
            await _db.SaveChangesAsync(ct);

            return Result<bool>.Success(true);
        }


        private static bool IsFinished(MatchStatus status) =>
            status == MatchStatus.FullTime;
    }
}
