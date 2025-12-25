using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Matches.Commands.UpdateMatch
{
    public class UpdateMatchCommandHandler : IRequestHandler<UpdateMatchCommand, Result<AdminMatchDto>>
    {
        private readonly ITipprDbContext _db;

        public UpdateMatchCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<AdminMatchDto>> Handle(UpdateMatchCommand request, CancellationToken cancellationToken)
        {
            var match = await _db.Matches
                .Include(m => m.Tournament)
                .Include(m => m.HomeTeam)
                .Include(m => m.AwayTeam)
                .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

            if (match == null)
                return Result<AdminMatchDto>.NotFound("Match not found", "admin.match_not_found");

            if (request.MatchDate.HasValue)
                match.MatchDate = request.MatchDate.Value;

            if (request.Stage.HasValue)
                match.Stage = request.Stage.Value;

            if (request.Status.HasValue)
                match.Status = request.Status.Value;

            if (request.Venue != null)
                match.VenueName = request.Venue;

            if (request.ApiFootballId.HasValue)
                match.ApiFootballId = request.ApiFootballId.Value;

            match.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            var dto = new AdminMatchDto
            {
                Id = match.Id,
                TournamentId = match.TournamentId,
                TournamentName = match.Tournament.Name,
                HomeTeamId = match.HomeTeamId,
                HomeTeamName = match.HomeTeam.Name,
                HomeTeamCode = match.HomeTeam.Code ?? string.Empty,
                HomeTeamLogoUrl = match.HomeTeam.LogoUrl,
                AwayTeamId = match.AwayTeamId,
                AwayTeamName = match.AwayTeam.Name,
                AwayTeamCode = match.AwayTeam.Code ?? string.Empty,
                AwayTeamLogoUrl = match.AwayTeam.LogoUrl,
                MatchDate = match.MatchDate,
                Stage = match.Stage,
                HomeScore = match.HomeScore,
                AwayScore = match.AwayScore,
                Status = match.Status,
                Venue = match.VenueName,
                ApiFootballId = match.ApiFootballId,
                ResultVersion = match.ResultVersion,
                CreatedAt = match.CreatedAt,
                UpdatedAt = match.UpdatedAt,
                PredictionCount = await _db.Predictions.CountAsync(p => p.MatchId == match.Id, cancellationToken)
            };

            return Result<AdminMatchDto>.Success(dto);
        }
    }
}
