using Application.Common;
using Application.Common.Interfaces;
using Application.Features.BonusQuestions.Commands.ResolveBonusQuestion;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.BonusQuestions.Commands;

public sealed class ResolveBonusQuestionCommandHandlerTests
{
    private static Mock<IDbContextTransaction> CreateTransactionMock()
    {
        var txMock = new Mock<IDbContextTransaction>();
        txMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        txMock.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        txMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);
        return txMock;
    }

    [Fact]
    public async Task Handle_Should_Resolve_And_Award_Points_Via_StandingsService()
    {
        // Arrange
        var bonusQuestionId = Guid.NewGuid();
        var correctTeamId = Guid.NewGuid();

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

        var team = new Team
        {
            Id = correctTeamId,
            Name = "France",
            TournamentId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var txMock = CreateTransactionMock();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.BonusQuestions).Returns(new List<BonusQuestion> { bonusQuestion }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Teams).Returns(new List<Team> { team }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        dbMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(txMock.Object);

        var standingsServiceMock = new Mock<IStandingsService>();
        standingsServiceMock
            .Setup(x => x.ScoreBonusPredictionsAsync(bonusQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var handler = new ResolveBonusQuestionCommandHandler(dbMock.Object, standingsServiceMock.Object);

        var cmd = new ResolveBonusQuestionCommand(
            BonusQuestionId: bonusQuestionId,
            AnswerTeamId: correctTeamId,
            AnswerText: null
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(2); // 2 correct predictions from standings service

        bonusQuestion.IsResolved.Should().BeTrue();
        bonusQuestion.AnswerTeamId.Should().Be(correctTeamId);

        standingsServiceMock.Verify(
            x => x.ScoreBonusPredictionsAsync(bonusQuestionId, It.IsAny<CancellationToken>()),
            Times.Once);
        txMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Resolve_With_Text_Answer()
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

        var txMock = CreateTransactionMock();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.BonusQuestions).Returns(new List<BonusQuestion> { bonusQuestion }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Teams).Returns(new List<Team>().BuildMockDbSet().Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        dbMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(txMock.Object);

        var standingsServiceMock = new Mock<IStandingsService>();
        standingsServiceMock
            .Setup(x => x.ScoreBonusPredictionsAsync(bonusQuestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var handler = new ResolveBonusQuestionCommandHandler(dbMock.Object, standingsServiceMock.Object);

        var cmd = new ResolveBonusQuestionCommand(
            BonusQuestionId: bonusQuestionId,
            AnswerTeamId: null,
            AnswerText: "Mbappe"
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(3);

        bonusQuestion.IsResolved.Should().BeTrue();
        bonusQuestion.AnswerText.Should().Be("Mbappe");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_BonusQuestion_Not_Found()
    {
        // Arrange
        var bonusQuestionId = Guid.NewGuid();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.BonusQuestions).Returns(new List<BonusQuestion>().BuildMockDbSet().Object);

        var standingsServiceMock = new Mock<IStandingsService>();

        var handler = new ResolveBonusQuestionCommandHandler(dbMock.Object, standingsServiceMock.Object);

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

        var standingsServiceMock = new Mock<IStandingsService>();

        var handler = new ResolveBonusQuestionCommandHandler(dbMock.Object, standingsServiceMock.Object);

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

        var standingsServiceMock = new Mock<IStandingsService>();

        var handler = new ResolveBonusQuestionCommandHandler(dbMock.Object, standingsServiceMock.Object);

        var cmd = new ResolveBonusQuestionCommand(bonusQuestionId, null, null); // no answer

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.BusinessRule);
        result.Error!.Code.Should().Be("bonus_question.answer_required");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Team_Not_Found()
    {
        // Arrange
        var bonusQuestionId = Guid.NewGuid();
        var nonExistentTeamId = Guid.NewGuid();

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
        dbMock.Setup(x => x.Teams).Returns(new List<Team>().BuildMockDbSet().Object);

        var standingsServiceMock = new Mock<IStandingsService>();

        var handler = new ResolveBonusQuestionCommandHandler(dbMock.Object, standingsServiceMock.Object);

        var cmd = new ResolveBonusQuestionCommand(bonusQuestionId, nonExistentTeamId, null);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error!.Code.Should().Be("team.not_found");
    }
}
