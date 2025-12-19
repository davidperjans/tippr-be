using Application.Common;
using Application.Common.Interfaces;
using Application.Features.BonusQuestions.Commands.ResolveBonusQuestion;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.BonusQuestions.Commands;

public sealed class ResolveBonusQuestionCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Resolve_And_Award_Points_To_Correct_Predictions()
    {
        // Arrange
        var bonusQuestionId = Guid.NewGuid();
        var correctTeamId = Guid.NewGuid();
        var wrongTeamId = Guid.NewGuid();

        var bonusQuestion = new BonusQuestion
        {
            Id = bonusQuestionId,
            TournamentId = Guid.NewGuid(),
            QuestionType = BonusQuestionType.Winner,
            Question = "Who will win?",
            Points = 10,
            IsResolved = false,
            CreatedAt = DateTime.UtcNow
        };

        var predictions = new List<BonusPrediction>
        {
            new BonusPrediction
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                LeagueId = Guid.NewGuid(),
                BonusQuestionId = bonusQuestionId,
                AnswerTeamId = correctTeamId, // correct
                PointsEarned = null,
                CreatedAt = DateTime.UtcNow
            },
            new BonusPrediction
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                LeagueId = Guid.NewGuid(),
                BonusQuestionId = bonusQuestionId,
                AnswerTeamId = wrongTeamId, // wrong
                PointsEarned = null,
                CreatedAt = DateTime.UtcNow
            },
            new BonusPrediction
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                LeagueId = Guid.NewGuid(),
                BonusQuestionId = bonusQuestionId,
                AnswerTeamId = correctTeamId, // correct
                PointsEarned = null,
                CreatedAt = DateTime.UtcNow
            }
        };

        var team = new Team
        {
            Id = correctTeamId,
            Name = "France",
            TournamentId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.BonusQuestions).Returns(new List<BonusQuestion> { bonusQuestion }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.BonusPredictions).Returns(predictions.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Teams).Returns(new List<Team> { team }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new ResolveBonusQuestionCommandHandler(dbMock.Object);

        var cmd = new ResolveBonusQuestionCommand(
            BonusQuestionId: bonusQuestionId,
            AnswerTeamId: correctTeamId,
            AnswerText: null
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(2); // 2 correct predictions

        bonusQuestion.IsResolved.Should().BeTrue();
        bonusQuestion.AnswerTeamId.Should().Be(correctTeamId);

        predictions[0].PointsEarned.Should().Be(10); // correct
        predictions[1].PointsEarned.Should().Be(0);  // wrong
        predictions[2].PointsEarned.Should().Be(10); // correct

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Match_Text_Answers_Case_Insensitive()
    {
        // Arrange
        var bonusQuestionId = Guid.NewGuid();

        var bonusQuestion = new BonusQuestion
        {
            Id = bonusQuestionId,
            TournamentId = Guid.NewGuid(),
            QuestionType = BonusQuestionType.TopScorer,
            Question = "Who will be the top scorer?",
            Points = 15,
            IsResolved = false,
            CreatedAt = DateTime.UtcNow
        };

        var predictions = new List<BonusPrediction>
        {
            new BonusPrediction
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                LeagueId = Guid.NewGuid(),
                BonusQuestionId = bonusQuestionId,
                AnswerText = "Mbappe", // correct (different case)
                PointsEarned = null,
                CreatedAt = DateTime.UtcNow
            },
            new BonusPrediction
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                LeagueId = Guid.NewGuid(),
                BonusQuestionId = bonusQuestionId,
                AnswerText = "MBAPPE", // correct (different case)
                PointsEarned = null,
                CreatedAt = DateTime.UtcNow
            },
            new BonusPrediction
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                LeagueId = Guid.NewGuid(),
                BonusQuestionId = bonusQuestionId,
                AnswerText = "Ronaldo", // wrong
                PointsEarned = null,
                CreatedAt = DateTime.UtcNow
            }
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.BonusQuestions).Returns(new List<BonusQuestion> { bonusQuestion }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.BonusPredictions).Returns(predictions.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Teams).Returns(new List<Team>().BuildMockDbSet().Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new ResolveBonusQuestionCommandHandler(dbMock.Object);

        var cmd = new ResolveBonusQuestionCommand(
            BonusQuestionId: bonusQuestionId,
            AnswerTeamId: null,
            AnswerText: "mbappe" // lowercase
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(2); // 2 correct predictions

        predictions[0].PointsEarned.Should().Be(15);
        predictions[1].PointsEarned.Should().Be(15);
        predictions[2].PointsEarned.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_BonusQuestion_Not_Found()
    {
        // Arrange
        var bonusQuestionId = Guid.NewGuid();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.BonusQuestions).Returns(new List<BonusQuestion>().BuildMockDbSet().Object);

        var handler = new ResolveBonusQuestionCommandHandler(dbMock.Object);

        var cmd = new ResolveBonusQuestionCommand(bonusQuestionId, Guid.NewGuid(), null);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error!.Code.Should().Be("bonus_question.not_found");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Already_Resolved()
    {
        // Arrange
        var bonusQuestionId = Guid.NewGuid();

        var bonusQuestion = new BonusQuestion
        {
            Id = bonusQuestionId,
            TournamentId = Guid.NewGuid(),
            QuestionType = BonusQuestionType.Winner,
            Question = "Who will win?",
            Points = 10,
            IsResolved = true, // already resolved
            AnswerTeamId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.BonusQuestions).Returns(new List<BonusQuestion> { bonusQuestion }.BuildMockDbSet().Object);

        var handler = new ResolveBonusQuestionCommandHandler(dbMock.Object);

        var cmd = new ResolveBonusQuestionCommand(bonusQuestionId, Guid.NewGuid(), null);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.BusinessRule);
        result.Error!.Code.Should().Be("bonus_question.already_resolved");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_No_Answer_Provided()
    {
        // Arrange
        var bonusQuestionId = Guid.NewGuid();

        var bonusQuestion = new BonusQuestion
        {
            Id = bonusQuestionId,
            TournamentId = Guid.NewGuid(),
            QuestionType = BonusQuestionType.Winner,
            Question = "Who will win?",
            Points = 10,
            IsResolved = false,
            CreatedAt = DateTime.UtcNow
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.BonusQuestions).Returns(new List<BonusQuestion> { bonusQuestion }.BuildMockDbSet().Object);

        var handler = new ResolveBonusQuestionCommandHandler(dbMock.Object);

        var cmd = new ResolveBonusQuestionCommand(bonusQuestionId, null, null); // no answer

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.BusinessRule);
        result.Error!.Code.Should().Be("bonus_question.answer_required");
    }
}
