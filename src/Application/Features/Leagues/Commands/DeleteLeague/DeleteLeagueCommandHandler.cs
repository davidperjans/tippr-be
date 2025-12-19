using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Leagues.Commands.DeleteLeague
{
    public sealed class DeleteLeagueCommandHandler : IRequestHandler<DeleteLeagueCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;

        public DeleteLeagueCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<bool>> Handle(DeleteLeagueCommand request, CancellationToken ct)
        {
            var league = await _db.Leagues
                .FirstOrDefaultAsync(l => l.Id == request.LeagueId, ct);

            if (league == null)
                return Result<bool>.NotFound("League not found.", "league.not_found");

            if (league.OwnerId != request.UserId)
                return Result<bool>.Forbidden("Only the league owner can delete the league.", "league.forbidden");

            if (league.IsGlobal)
                return Result<bool>.Forbidden("Global leagues cannot be deleted.", "league.global_delete_forbidden");

            _db.Leagues.Remove(league);
            await _db.SaveChangesAsync(ct);

            return Result<bool>.Success(true);
        }
    }
}
