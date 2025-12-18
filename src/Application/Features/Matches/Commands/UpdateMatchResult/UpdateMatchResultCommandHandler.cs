using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Matches.Commands.UpdateMatchResult
{
    public sealed class UpdateMatchResultCommandHandler : IRequestHandler<UpdateMatchResultCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;

        public UpdateMatchResultCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<bool>> Handle(UpdateMatchResultCommand request, CancellationToken ct)
        {
            var match = await _db.Matches.FirstOrDefaultAsync(m => m.Id == request.MatchId, ct);
            if (match is null)
                return Result<bool>.Failure("match not found.");

            match.HomeScore = request.HomeScore;
            match.AwayScore = request.AwayScore;
            match.Status = request.Status;
            match.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            // (Senare sprint) trigga poängberäkning + uppdatera LeagueStandings i batch
            // DB-guiden nämner batch updates efter match. :contentReference[oaicite:9]{index=9}

            return Result<bool>.Success(true);
        }
    }
}
