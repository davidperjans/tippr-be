using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Teams.Commands.BulkCreateTeams
{
    public class BulkCreateTeamsCommandHandler : IRequestHandler<BulkCreateTeamsCommand, Result<BulkCreateTeamsResult>>
    {
        private readonly ITipprDbContext _db;

        public BulkCreateTeamsCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<BulkCreateTeamsResult>> Handle(BulkCreateTeamsCommand request, CancellationToken cancellationToken)
        {
            var tournamentExists = await _db.Tournaments
                .AnyAsync(t => t.Id == request.TournamentId, cancellationToken);

            if (!tournamentExists)
                return Result<BulkCreateTeamsResult>.NotFound("Tournament not found", "admin.tournament_not_found");

            // Get existing teams in tournament
            var existingTeams = await _db.Teams
                .Where(t => t.TournamentId == request.TournamentId)
                .Select(t => new { t.Name, t.Code })
                .ToListAsync(cancellationToken);

            var existingNames = existingTeams.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var existingCodes = existingTeams.Select(t => t.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var result = new BulkCreateTeamsResult
            {
                CreatedCount = 0,
                SkippedCount = 0,
                SkippedTeams = new List<string>(),
                CreatedIds = new List<Guid>()
            };

            var teamsToAdd = new List<Team>();

            foreach (var teamItem in request.Teams)
            {
                if (existingNames.Contains(teamItem.Name) || existingCodes.Contains(teamItem.Code))
                {
                    result.SkippedTeams.Add($"{teamItem.Name} ({teamItem.Code})");
                    continue;
                }

                var team = new Team
                {
                    Id = Guid.NewGuid(),
                    TournamentId = request.TournamentId,
                    Name = teamItem.Name,
                    Code = teamItem.Code,
                    FlagUrl = teamItem.FlagUrl,
                    GroupName = teamItem.GroupName,
                    FifaRank = teamItem.FifaRank,
                    FifaPoints = teamItem.FifaPoints,
                    FifaRankingUpdatedAt = teamItem.FifaRank.HasValue ? DateTime.UtcNow : null,
                    ApiFootballId = teamItem.ApiFootballId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                teamsToAdd.Add(team);
                result.CreatedIds.Add(team.Id);

                // Add to existing sets to prevent duplicates within the same batch
                existingNames.Add(teamItem.Name);
                existingCodes.Add(teamItem.Code);
            }

            if (teamsToAdd.Any())
            {
                _db.Teams.AddRange(teamsToAdd);
                await _db.SaveChangesAsync(cancellationToken);
            }

            return Result<BulkCreateTeamsResult>.Success(new BulkCreateTeamsResult
            {
                CreatedCount = teamsToAdd.Count,
                SkippedCount = request.Teams.Count - teamsToAdd.Count,
                SkippedTeams = result.SkippedTeams,
                CreatedIds = result.CreatedIds
            });
        }
    }
}
