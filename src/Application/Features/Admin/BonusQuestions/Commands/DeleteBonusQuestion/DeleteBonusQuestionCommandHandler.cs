using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.BonusQuestions.Commands.DeleteBonusQuestion
{
    public class DeleteBonusQuestionCommandHandler : IRequestHandler<DeleteBonusQuestionCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;
        private readonly IStandingsService _standingsService;

        public DeleteBonusQuestionCommandHandler(ITipprDbContext db, IStandingsService standingsService)
        {
            _db = db;
            _standingsService = standingsService;
        }

        public async Task<Result<bool>> Handle(DeleteBonusQuestionCommand request, CancellationToken cancellationToken)
        {
            var question = await _db.BonusQuestions
                .Include(bq => bq.Predictions)
                .FirstOrDefaultAsync(bq => bq.Id == request.BonusQuestionId, cancellationToken);

            if (question == null)
                return Result<bool>.NotFound("Bonus question not found", "admin.bonus_question_not_found");

            // Get affected league IDs before deletion
            var affectedLeagueIds = question.Predictions.Select(p => p.LeagueId).Distinct().ToList();

            // Remove all predictions for this question
            _db.BonusPredictions.RemoveRange(question.Predictions);
            _db.BonusQuestions.Remove(question);

            await _db.SaveChangesAsync(cancellationToken);

            // Recalculate standings for affected leagues
            foreach (var leagueId in affectedLeagueIds)
            {
                await _standingsService.RecalculateRanksForLeagueAsync(leagueId, cancellationToken);
            }

            return Result<bool>.Success(true);
        }
    }
}
