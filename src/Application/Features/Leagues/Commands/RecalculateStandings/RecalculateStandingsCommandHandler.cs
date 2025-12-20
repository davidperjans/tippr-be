using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Leagues.Commands.RecalculateStandings
{
    public sealed class RecalculateStandingsCommandHandler : IRequestHandler<RecalculateStandingsCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;
        private readonly IStandingsService _standingsService;

        public RecalculateStandingsCommandHandler(ITipprDbContext db, IStandingsService standingsService)
        {
            _db = db;
            _standingsService = standingsService;
        }

        public async Task<Result<bool>> Handle(RecalculateStandingsCommand request, CancellationToken ct)
        {
            var leagueExists = await _db.Leagues
                .AnyAsync(l => l.Id == request.LeagueId, ct);

            if (!leagueExists)
                return Result<bool>.NotFound("League not found", "league.not_found");

            await _standingsService.RecalculateStandingsForLeagueAsync(request.LeagueId, ct);

            return Result<bool>.Success(true);
        }
    }
}
