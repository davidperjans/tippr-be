using Application.Common;
using Application.Common.Interfaces;
using Application.Features.BonusQuestions.Commands.CreateBonusQuestion;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.BonusQuestions.Commands;

public sealed class CreateBonusQuestionCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_BonusQuestion_And_Return_Id()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();

        var tournaments = new List<Tournament>
        {
            new Tournament
            {
                Id = tournamentId,
                Name = "Euro 2024",
                Year = 2024,
                Type = TournamentType.EuroCup,
                StartDate = DateTime.UtcNow.AddMonths(1),
                EndDate = DateTime.UtcNow.AddMonths(2),
                Country = "Germany",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var bonusQuestions = new List<BonusQuestion>();
        var bonusQuestionsDbSetMock = bonusQuestions.BuildMockDbSet();

        bonusQuestionsDbSetMock
            .Setup(x => x.Add(It.IsAny<BonusQuestion>()))
            .Callback<BonusQuestion>(bq => bonusQuestions.Add(bq));

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Tournaments).Returns(tournaments.BuildMockDbSet().Object);
        dbMock.Setup(x => x.BonusQuestions).Returns(bonusQuestionsDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateBonusQuestionCommandHandler(dbMock.Object);

        var cmd = new CreateBonusQuestionCommand(
            TournamentId: tournamentId,
            QuestionType: BonusQuestionType.Winner,
            Question: "Who will win Euro 2024?",
            Points: 10
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);

        bonusQuestions.Should().HaveCount(1);
        bonusQuestions[0].TournamentId.Should().Be(tournamentId);
        bonusQuestions[0].QuestionType.Should().Be(BonusQuestionType.Winner);
        bonusQuestions[0].Question.Should().Be("Who will win Euro 2024?");
        bonusQuestions[0].Points.Should().Be(10);
        bonusQuestions[0].IsResolved.Should().BeFalse();

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Tournament_Not_Found()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();

        var tournaments = new List<Tournament>(); // empty

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Tournaments).Returns(tournaments.BuildMockDbSet().Object);
        dbMock.Setup(x => x.BonusQuestions).Returns(new List<BonusQuestion>().BuildMockDbSet().Object);

        var handler = new CreateBonusQuestionCommandHandler(dbMock.Object);

        var cmd = new CreateBonusQuestionCommand(
            TournamentId: tournamentId,
            QuestionType: BonusQuestionType.Winner,
            Question: "Who will win?",
            Points: 10
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error!.Code.Should().Be("tournament.not_found");

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_BonusQuestion_Of_Same_Type_Already_Exists()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();

        var tournaments = new List<Tournament>
        {
            new Tournament
            {
                Id = tournamentId,
                Name = "Euro 2024",
                Year = 2024,
                Type = TournamentType.EuroCup,
                StartDate = DateTime.UtcNow.AddMonths(1),
                EndDate = DateTime.UtcNow.AddMonths(2),
                Country = "Germany",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var bonusQuestions = new List<BonusQuestion>
        {
            new BonusQuestion
            {
                Id = Guid.NewGuid(),
                TournamentId = tournamentId,
                QuestionType = BonusQuestionType.Winner,
                Question = "Who will win Euro 2024?",
                Points = 10,
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Tournaments).Returns(tournaments.BuildMockDbSet().Object);
        dbMock.Setup(x => x.BonusQuestions).Returns(bonusQuestions.BuildMockDbSet().Object);

        var handler = new CreateBonusQuestionCommandHandler(dbMock.Object);

        var cmd = new CreateBonusQuestionCommand(
            TournamentId: tournamentId,
            QuestionType: BonusQuestionType.Winner, // same type
            Question: "Different question same type",
            Points: 15
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error!.Code.Should().Be("bonus_question.already_exists");

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
