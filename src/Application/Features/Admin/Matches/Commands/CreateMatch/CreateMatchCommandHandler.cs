using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Matches.Commands.CreateMatch
{
    public class CreateMatchCommandHandler : IRequestHandler<CreateMatchCommand, Result<Guid>>
    {
        private readonly ITipprDbContext _db;

        public CreateMatchCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<Guid>> Handle(CreateMatchCommand request, CancellationToken cancellationToken)
        {
            var tournamentExists = await _db.Tournaments
                .AnyAsync(t => t.Id == request.TournamentId, cancellationToken);

            if (!tournamentExists)
                return Result<Guid>.NotFound("Tournament not found", "admin.tournament_not_found");

            var homeTeamExists = await _db.Teams
                .AnyAsync(t => t.Id == request.HomeTeamId && t.TournamentId == request.TournamentId, cancellationToken);

            if (!homeTeamExists)
                return Result<Guid>.NotFound("Home team not found in this tournament", "admin.home_team_not_found");

            var awayTeamExists = await _db.Teams
                .AnyAsync(t => t.Id == request.AwayTeamId && t.TournamentId == request.TournamentId, cancellationToken);

            if (!awayTeamExists)
                return Result<Guid>.NotFound("Away team not found in this tournament", "admin.away_team_not_found");

            if (request.HomeTeamId == request.AwayTeamId)
                return Result<Guid>.BusinessRule("Home team and away team cannot be the same", "admin.same_teams");

            var match = new Match
            {
                Id = Guid.NewGuid(),
                TournamentId = request.TournamentId,
                HomeTeamId = request.HomeTeamId,
                AwayTeamId = request.AwayTeamId,
                MatchDate = request.MatchDate,
                Stage = request.Stage,
                Status = MatchStatus.Scheduled,
                Venue = request.Venue,
                ApiFootballId = request.ApiFootballId,
                ResultVersion = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Matches.Add(match);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(match.Id);
        }
    }
}
