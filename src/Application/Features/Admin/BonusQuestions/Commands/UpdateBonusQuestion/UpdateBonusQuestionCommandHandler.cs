using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.BonusQuestions.Commands.UpdateBonusQuestion
{
    public class UpdateBonusQuestionCommandHandler : IRequestHandler<UpdateBonusQuestionCommand, Result<AdminBonusQuestionDto>>
    {
        private readonly ITipprDbContext _db;

        public UpdateBonusQuestionCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<AdminBonusQuestionDto>> Handle(UpdateBonusQuestionCommand request, CancellationToken cancellationToken)
        {
            var question = await _db.BonusQuestions
                .Include(bq => bq.Tournament)
                .Include(bq => bq.AnswerTeam)
                .FirstOrDefaultAsync(bq => bq.Id == request.BonusQuestionId, cancellationToken);

            if (question == null)
                return Result<AdminBonusQuestionDto>.NotFound("Bonus question not found", "admin.bonus_question_not_found");

            if (question.IsResolved)
                return Result<AdminBonusQuestionDto>.BusinessRule("Cannot update a resolved bonus question", "admin.bonus_question_resolved");

            if (request.QuestionType.HasValue)
                question.QuestionType = request.QuestionType.Value;

            if (!string.IsNullOrWhiteSpace(request.Question))
                question.Question = request.Question;

            if (request.Points.HasValue)
                question.Points = request.Points.Value;

            question.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            var dto = new AdminBonusQuestionDto
            {
                Id = question.Id,
                TournamentId = question.TournamentId,
                TournamentName = question.Tournament.Name,
                QuestionType = question.QuestionType,
                Question = question.Question,
                AnswerTeamId = question.AnswerTeamId,
                AnswerTeamName = question.AnswerTeam?.Name,
                AnswerText = question.AnswerText,
                IsResolved = question.IsResolved,
                Points = question.Points,
                CreatedAt = question.CreatedAt,
                UpdatedAt = question.UpdatedAt,
                PredictionCount = await _db.BonusPredictions.CountAsync(bp => bp.BonusQuestionId == question.Id, cancellationToken)
            };

            return Result<AdminBonusQuestionDto>.Success(dto);
        }
    }
}
