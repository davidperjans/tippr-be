using Application.Common;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.BonusQuestions.Commands.RecalculateBonusQuestion
{
    public class RecalculateBonusQuestionCommandHandler : IRequestHandler<RecalculateBonusQuestionCommand, Result<RecalculateBonusQuestionResult>>
    {
        private readonly ITipprDbContext _db;
        private readonly IStandingsService _standingsService;

        public RecalculateBonusQuestionCommandHandler(ITipprDbContext db, IStandingsService standingsService)
        {
            _db = db;
            _standingsService = standingsService;
        }

        public async Task<Result<RecalculateBonusQuestionResult>> Handle(RecalculateBonusQuestionCommand request, CancellationToken cancellationToken)
        {
            var question = await _db.BonusQuestions
                .FirstOrDefaultAsync(bq => bq.Id == request.BonusQuestionId, cancellationToken);

            if (question == null)
                return Result<RecalculateBonusQuestionResult>.NotFound("Bonus question not found", "admin.bonus_question_not_found");

            if (!question.IsResolved)
                return Result<RecalculateBonusQuestionResult>.BusinessRule("Bonus question must be resolved to recalculate", "admin.bonus_question_not_resolved");

            var predictions = await _db.BonusPredictions
                .Where(bp => bp.BonusQuestionId == request.BonusQuestionId)
                .ToListAsync(cancellationToken);

            var affectedLeagueIds = new HashSet<Guid>();
            int correctCount = 0;
            int totalPoints = 0;

            foreach (var prediction in predictions)
            {
                bool isCorrect = false;

                // Check if this is a team-based question
                if (question.AnswerTeamId.HasValue)
                {
                    isCorrect = prediction.AnswerTeamId == question.AnswerTeamId;
                }
                else if (!string.IsNullOrEmpty(question.AnswerText))
                {
                    isCorrect = string.Equals(prediction.AnswerText?.Trim(), question.AnswerText.Trim(), StringComparison.OrdinalIgnoreCase);
                }

                prediction.PointsEarned = isCorrect ? question.Points : 0;

                if (isCorrect)
                {
                    correctCount++;
                    totalPoints += question.Points;
                }

                affectedLeagueIds.Add(prediction.LeagueId);
            }

            await _db.SaveChangesAsync(cancellationToken);

            // Recalculate standings for all affected leagues
            foreach (var leagueId in affectedLeagueIds)
            {
                await _standingsService.RecalculateRanksForLeagueAsync(leagueId, cancellationToken);
            }

            return Result<RecalculateBonusQuestionResult>.Success(new RecalculateBonusQuestionResult
            {
                PredictionsUpdated = predictions.Count,
                CorrectPredictions = correctCount,
                TotalPointsAwarded = totalPoints,
                LeaguesAffected = affectedLeagueIds.Count
            });
        }
    }
}
