using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.BonusQuestions.Commands.CreateBonusQuestion
{
    public sealed class CreateBonusQuestionCommandHandler : IRequestHandler<CreateBonusQuestionCommand, Result<Guid>>
    {
        private readonly ITipprDbContext _db;

        public CreateBonusQuestionCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<Guid>> Handle(CreateBonusQuestionCommand request, CancellationToken ct)
        {
            var tournamentExists = await _db.Tournaments
                .AnyAsync(t => t.Id == request.TournamentId, ct);

            if (!tournamentExists)
                return Result<Guid>.NotFound("Tournament not found", "tournament.not_found");

            var exists = await _db.BonusQuestions
                .AnyAsync(bq => bq.TournamentId == request.TournamentId
                    && bq.QuestionType == request.QuestionType, ct);

            if (exists)
                return Result<Guid>.Conflict("Bonus question of this type already exists for this tournament", "bonus_question.already_exists");

            var bonusQuestion = new BonusQuestion
            {
                Id = Guid.NewGuid(),
                TournamentId = request.TournamentId,
                QuestionType = request.QuestionType,
                Question = request.Question,
                Points = request.Points,
                IsResolved = false
            };

            _db.BonusQuestions.Add(bonusQuestion);
            await _db.SaveChangesAsync(ct);

            return Result<Guid>.Success(bonusQuestion.Id);
        }
    }
}
