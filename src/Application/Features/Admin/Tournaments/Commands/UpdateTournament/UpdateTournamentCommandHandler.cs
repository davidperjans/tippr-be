using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Tournaments.Commands.UpdateTournament
{
    public class UpdateTournamentCommandHandler : IRequestHandler<UpdateTournamentCommand, Result<AdminTournamentDto>>
    {
        private readonly ITipprDbContext _db;

        public UpdateTournamentCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<AdminTournamentDto>> Handle(UpdateTournamentCommand request, CancellationToken cancellationToken)
        {
            var tournament = await _db.Tournaments
                .FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);

            if (tournament == null)
                return Result<AdminTournamentDto>.NotFound("Tournament not found", "admin.tournament_not_found");

            if (!string.IsNullOrWhiteSpace(request.Name))
                tournament.Name = request.Name;

            if (request.Year.HasValue)
                tournament.Year = request.Year.Value;

            if (request.Type.HasValue)
                tournament.Type = request.Type.Value;

            if (request.StartDate.HasValue)
                tournament.StartDate = request.StartDate.Value;

            if (request.EndDate.HasValue)
                tournament.EndDate = request.EndDate.Value;

            if (request.LogoUrl != null)
                tournament.LogoUrl = request.LogoUrl;

            tournament.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            var dto = new AdminTournamentDto
            {
                Id = tournament.Id,
                Name = tournament.Name,
                Year = tournament.Year,
                Type = tournament.Type,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                LogoUrl = tournament.LogoUrl,
                IsActive = tournament.IsActive,
                CreatedAt = tournament.CreatedAt,
                UpdatedAt = tournament.UpdatedAt,
                TeamCount = await _db.Teams.CountAsync(t => t.TournamentId == tournament.Id, cancellationToken),
                MatchCount = await _db.Matches.CountAsync(m => m.TournamentId == tournament.Id, cancellationToken),
                LeagueCount = await _db.Leagues.CountAsync(l => l.TournamentId == tournament.Id, cancellationToken),
                BonusQuestionCount = await _db.BonusQuestions.CountAsync(bq => bq.TournamentId == tournament.Id, cancellationToken)
            };

            return Result<AdminTournamentDto>.Success(dto);
        }
    }
}
