using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.BonusQuestions.Commands.ResolveBonusQuestion
{
    public sealed class ResolveBonusQuestionCommandHandler : IRequestHandler<ResolveBonusQuestionCommand, Result<int>>
    {
        private readonly ITipprDbContext _db;
        private readonly IStandingsService _standingsService;

        public ResolveBonusQuestionCommandHandler(ITipprDbContext db, IStandingsService standingsService)
        {
            _db = db;
            _standingsService = standingsService;
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

            await using var tx = await _db.BeginTransactionAsync(ct);

            try
            {
                // Update the bonus question with the correct answer
                bonusQuestion.AnswerTeamId = request.AnswerTeamId;
                bonusQuestion.AnswerText = request.AnswerText;
                bonusQuestion.IsResolved = true;
                bonusQuestion.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync(ct);

                // Score all bonus predictions and update standings
                var awardedCount = await _standingsService.ScoreBonusPredictionsAsync(
                    request.BonusQuestionId,
                    ct);

                await tx.CommitAsync(ct);

                return Result<int>.Success(awardedCount);
            }
            catch (Exception)
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
    }
}
