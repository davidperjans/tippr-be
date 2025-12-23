using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Teams.Commands.CreateTeam
{
    public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, Result<Guid>>
    {
        private readonly ITipprDbContext _db;

        public CreateTeamCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<Guid>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
        {
            var tournamentExists = await _db.Tournaments
                .AnyAsync(t => t.Id == request.TournamentId, cancellationToken);

            if (!tournamentExists)
                return Result<Guid>.NotFound("Tournament not found", "admin.tournament_not_found");

            // Check for duplicate team in tournament
            var exists = await _db.Teams
                .AnyAsync(t => t.TournamentId == request.TournamentId &&
                    (t.Name == request.Name || t.Code == request.Code), cancellationToken);

            if (exists)
                return Result<Guid>.Conflict("Team with this name or code already exists in the tournament", "admin.team_exists");

            var team = new Team
            {
                Id = Guid.NewGuid(),
                TournamentId = request.TournamentId,
                Name = request.Name,
                Code = request.Code,
                FlagUrl = request.FlagUrl,
                GroupName = request.GroupName,
                FifaRank = request.FifaRank,
                FifaPoints = request.FifaPoints,
                FifaRankingUpdatedAt = request.FifaRank.HasValue ? DateTime.UtcNow : null,
                ApiFootballId = request.ApiFootballId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Teams.Add(team);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(team.Id);
        }
    }
}
