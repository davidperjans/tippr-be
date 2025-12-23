using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Matches.Commands.BulkCreateMatches
{
    public class BulkCreateMatchesCommandHandler : IRequestHandler<BulkCreateMatchesCommand, Result<BulkCreateMatchesResult>>
    {
        private readonly ITipprDbContext _db;

        public BulkCreateMatchesCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<BulkCreateMatchesResult>> Handle(BulkCreateMatchesCommand request, CancellationToken cancellationToken)
        {
            var tournamentExists = await _db.Tournaments
                .AnyAsync(t => t.Id == request.TournamentId, cancellationToken);

            if (!tournamentExists)
                return Result<BulkCreateMatchesResult>.NotFound("Tournament not found", "admin.tournament_not_found");

            // Get all valid team IDs for this tournament
            var teamIdsList = await _db.Teams
                .Where(t => t.TournamentId == request.TournamentId)
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);
            var validTeamIds = teamIdsList.ToHashSet();

            var result = new BulkCreateMatchesResult
            {
                CreatedCount = 0,
                FailedCount = 0,
                Errors = new List<string>(),
                CreatedIds = new List<Guid>()
            };

            var matchesToAdd = new List<Match>();

            for (int i = 0; i < request.Matches.Count; i++)
            {
                var matchItem = request.Matches[i];

                if (!validTeamIds.Contains(matchItem.HomeTeamId))
                {
                    result.Errors.Add($"Match {i + 1}: Home team not found in tournament");
                    continue;
                }

                if (!validTeamIds.Contains(matchItem.AwayTeamId))
                {
                    result.Errors.Add($"Match {i + 1}: Away team not found in tournament");
                    continue;
                }

                if (matchItem.HomeTeamId == matchItem.AwayTeamId)
                {
                    result.Errors.Add($"Match {i + 1}: Home and away team cannot be the same");
                    continue;
                }

                var match = new Match
                {
                    Id = Guid.NewGuid(),
                    TournamentId = request.TournamentId,
                    HomeTeamId = matchItem.HomeTeamId,
                    AwayTeamId = matchItem.AwayTeamId,
                    MatchDate = matchItem.MatchDate,
                    Stage = matchItem.Stage,
                    Status = MatchStatus.Scheduled,
                    Venue = matchItem.Venue,
                    ApiFootballId = matchItem.ApiFootballId,
                    ResultVersion = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                matchesToAdd.Add(match);
                result.CreatedIds.Add(match.Id);
            }

            if (matchesToAdd.Any())
            {
                _db.Matches.AddRange(matchesToAdd);
                await _db.SaveChangesAsync(cancellationToken);
            }

            return Result<BulkCreateMatchesResult>.Success(new BulkCreateMatchesResult
            {
                CreatedCount = matchesToAdd.Count,
                FailedCount = request.Matches.Count - matchesToAdd.Count,
                Errors = result.Errors,
                CreatedIds = result.CreatedIds
            });
        }
    }
}
