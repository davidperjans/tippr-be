using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.BonusQuestions.Commands.SubmitBonusPrediction
{
    public sealed class SubmitBonusPredictionCommandHandler : IRequestHandler<SubmitBonusPredictionCommand, Result<Guid>>
    {
        private readonly ITipprDbContext _db;
        private readonly ICurrentUser _currentUser;

        public SubmitBonusPredictionCommandHandler(ITipprDbContext db, ICurrentUser currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<Result<Guid>> Handle(SubmitBonusPredictionCommand request, CancellationToken ct)
        {
            var userId = _currentUser.UserId;

            var league = await _db.Leagues
                .Include(l => l.Tournament)
                .FirstOrDefaultAsync(l => l.Id == request.LeagueId, ct);

            if (league == null)
                return Result<Guid>.NotFound("League not found", "league.not_found");

            var isMember = await _db.LeagueMembers
                .AnyAsync(lm => lm.LeagueId == request.LeagueId && lm.UserId == userId, ct);

            if (!isMember)
                return Result<Guid>.Forbidden("You are not a member of this league", "league.not_member");

            var bonusQuestion = await _db.BonusQuestions
                .FirstOrDefaultAsync(bq => bq.Id == request.BonusQuestionId, ct);

            if (bonusQuestion == null)
                return Result<Guid>.NotFound("Bonus question not found", "bonus_question.not_found");

            if (bonusQuestion.TournamentId != league.TournamentId)
                return Result<Guid>.BusinessRule("Bonus question does not belong to this league's tournament", "bonus_question.tournament_mismatch");

            // Check deadline: bonus predictions must be submitted before tournament starts
            if (league.Tournament != null && DateTime.UtcNow >= league.Tournament.StartDate)
                return Result<Guid>.BusinessRule("Deadline passed - tournament has already started", "bonus_prediction.deadline_passed");

            if (bonusQuestion.IsResolved)
                return Result<Guid>.BusinessRule("This bonus question has already been resolved", "bonus_question.already_resolved");

            var exists = await _db.BonusPredictions
                .AnyAsync(bp => bp.UserId == userId
                    && bp.LeagueId == request.LeagueId
                    && bp.BonusQuestionId == request.BonusQuestionId, ct);

            if (exists)
                return Result<Guid>.Conflict("You have already submitted a prediction for this bonus question", "bonus_prediction.already_exists");

            // Validate that at least one answer is provided
            if (request.AnswerTeamId == null && string.IsNullOrWhiteSpace(request.AnswerText))
                return Result<Guid>.BusinessRule("An answer must be provided (either team or text)", "bonus_prediction.answer_required");

            // If AnswerTeamId is provided, validate the team exists
            if (request.AnswerTeamId.HasValue)
            {
                var teamExists = await _db.Teams
                    .AnyAsync(t => t.Id == request.AnswerTeamId.Value, ct);

                if (!teamExists)
                    return Result<Guid>.NotFound("Team not found", "team.not_found");
            }

            var bonusPrediction = new BonusPrediction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LeagueId = request.LeagueId,
                BonusQuestionId = request.BonusQuestionId,
                AnswerTeamId = request.AnswerTeamId,
                AnswerText = request.AnswerText,
                PointsEarned = null
            };

            _db.BonusPredictions.Add(bonusPrediction);
            await _db.SaveChangesAsync(ct);

            return Result<Guid>.Success(bonusPrediction.Id);
        }
    }
}
