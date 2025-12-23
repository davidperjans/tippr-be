using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Standings.Commands.RecalculateTournamentStandings
{
    public class RecalculateTournamentStandingsCommandHandler : IRequestHandler<RecalculateTournamentStandingsCommand, Result<RecalculateTournamentStandingsResult>>
    {
        private readonly ITipprDbContext _db;
        private readonly IStandingsService _standingsService;

        public RecalculateTournamentStandingsCommandHandler(ITipprDbContext db, IStandingsService standingsService)
        {
            _db = db;
            _standingsService = standingsService;
        }

        public async Task<Result<RecalculateTournamentStandingsResult>> Handle(RecalculateTournamentStandingsCommand request, CancellationToken cancellationToken)
        {
            var tournamentExists = await _db.Tournaments
                .AnyAsync(t => t.Id == request.TournamentId, cancellationToken);

            if (!tournamentExists)
                return Result<RecalculateTournamentStandingsResult>.NotFound("Tournament not found", "admin.tournament_not_found");

            // Get all leagues for this tournament
            var leagueIds = await _db.Leagues
                .Where(l => l.TournamentId == request.TournamentId)
                .Select(l => l.Id)
                .ToListAsync(cancellationToken);

            int totalMembersUpdated = 0;

            foreach (var leagueId in leagueIds)
            {
                var membersInLeague = await _db.LeagueMembers
                    .CountAsync(lm => lm.LeagueId == leagueId, cancellationToken);

                await _standingsService.RecalculateRanksForLeagueAsync(leagueId, cancellationToken);
                totalMembersUpdated += membersInLeague;
            }

            return Result<RecalculateTournamentStandingsResult>.Success(new RecalculateTournamentStandingsResult
            {
                LeaguesUpdated = leagueIds.Count,
                TotalMembersUpdated = totalMembersUpdated
            });
        }
    }
}
