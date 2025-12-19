using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.BonusQuestions.Commands.ResolveBonusQuestion
{
    public sealed class ResolveBonusQuestionCommandHandler : IRequestHandler<ResolveBonusQuestionCommand, Result<int>>
    {
        private readonly ITipprDbContext _db;

        public ResolveBonusQuestionCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<int>> Handle(ResolveBonusQuestionCommand request, CancellationToken ct)
        {
            var bonusQuestion = await _db.BonusQuestions
                .FirstOrDefaultAsync(bq => bq.Id == request.BonusQuestionId, ct);

            if (bonusQuestion == null)
                return Result<int>.NotFound("Bonus question not found", "bonus_question.not_found");

            if (bonusQuestion.IsResolved)
                return Result<int>.BusinessRule("This bonus question has already been resolved", "bonus_question.already_resolved");

            // Validate that at least one answer is provided
            if (request.AnswerTeamId == null && string.IsNullOrWhiteSpace(request.AnswerText))
                return Result<int>.BusinessRule("An answer must be provided (either team or text)", "bonus_question.answer_required");

            // If AnswerTeamId is provided, validate the team exists
            if (request.AnswerTeamId.HasValue)
            {
                var teamExists = await _db.Teams
                    .AnyAsync(t => t.Id == request.AnswerTeamId.Value, ct);

                if (!teamExists)
                    return Result<int>.NotFound("Team not found", "team.not_found");
            }

            // Update the bonus question with the correct answer
            bonusQuestion.AnswerTeamId = request.AnswerTeamId;
            bonusQuestion.AnswerText = request.AnswerText;
            bonusQuestion.IsResolved = true;

            // Get all predictions for this bonus question
            var predictions = await _db.BonusPredictions
                .Where(bp => bp.BonusQuestionId == request.BonusQuestionId)
                .ToListAsync(ct);

            var awardedCount = 0;

            foreach (var prediction in predictions)
            {
                var isCorrect = IsCorrectPrediction(prediction.AnswerTeamId, prediction.AnswerText, request.AnswerTeamId, request.AnswerText);

                if (isCorrect)
                {
                    prediction.PointsEarned = bonusQuestion.Points;
                    awardedCount++;
                }
                else
                {
                    prediction.PointsEarned = 0;
                }
            }

            await _db.SaveChangesAsync(ct);

            return Result<int>.Success(awardedCount);
        }

        private static bool IsCorrectPrediction(Guid? predictedTeamId, string? predictedText, Guid? correctTeamId, string? correctText)
        {
            // If the correct answer is a team, match by team ID
            if (correctTeamId.HasValue)
            {
                return predictedTeamId.HasValue && predictedTeamId.Value == correctTeamId.Value;
            }

            // If the correct answer is text, match by text (case-insensitive)
            if (!string.IsNullOrWhiteSpace(correctText))
            {
                return !string.IsNullOrWhiteSpace(predictedText) &&
                       string.Equals(predictedText.Trim(), correctText.Trim(), StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}
