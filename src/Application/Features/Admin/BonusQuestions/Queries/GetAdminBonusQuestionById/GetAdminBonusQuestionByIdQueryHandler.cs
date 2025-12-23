using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.BonusQuestions.Queries.GetAdminBonusQuestionById
{
    public class GetAdminBonusQuestionByIdQueryHandler : IRequestHandler<GetAdminBonusQuestionByIdQuery, Result<AdminBonusQuestionDto>>
    {
        private readonly ITipprDbContext _db;

        public GetAdminBonusQuestionByIdQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<AdminBonusQuestionDto>> Handle(GetAdminBonusQuestionByIdQuery request, CancellationToken cancellationToken)
        {
            var question = await _db.BonusQuestions
                .AsNoTracking()
                .Where(bq => bq.Id == request.BonusQuestionId)
                .Select(bq => new AdminBonusQuestionDto
                {
                    Id = bq.Id,
                    TournamentId = bq.TournamentId,
                    TournamentName = bq.Tournament.Name,
                    QuestionType = bq.QuestionType,
                    Question = bq.Question,
                    AnswerTeamId = bq.AnswerTeamId,
                    AnswerTeamName = bq.AnswerTeam != null ? bq.AnswerTeam.Name : null,
                    AnswerText = bq.AnswerText,
                    IsResolved = bq.IsResolved,
                    Points = bq.Points,
                    CreatedAt = bq.CreatedAt,
                    UpdatedAt = bq.UpdatedAt,
                    PredictionCount = bq.Predictions.Count
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (question == null)
                return Result<AdminBonusQuestionDto>.NotFound("Bonus question not found", "admin.bonus_question_not_found");

            return Result<AdminBonusQuestionDto>.Success(question);
        }
    }
}
