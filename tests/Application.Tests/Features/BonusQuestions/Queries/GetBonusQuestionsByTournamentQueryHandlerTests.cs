using Application.Common.Interfaces;
using Application.Features.BonusQuestions.Queries.GetBonusQuestionsByTournament;
using Application.Features.Predictions.Mapping;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.BonusQuestions.Queries;

public sealed class GetBonusQuestionsByTournamentQueryHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<BonusQuestionProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_Return_BonusQuestions_For_Tournament()
    {
        // Arrange
        var mapper = CreateMapper();
        var tournamentId = Guid.NewGuid();
        var otherTournamentId = Guid.NewGuid();

        var bonusQuestions = new List<BonusQuestion>
        {
            new BonusQuestion
            {
                Id = Guid.NewGuid(),
                TournamentId = tournamentId,
                QuestionType = BonusQuestionType.Winner,
                Question = "Who will win?",
                Points = 10,
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            },
            new BonusQuestion
            {
                Id = Guid.NewGuid(),
                TournamentId = tournamentId,
                QuestionType = BonusQuestionType.TopScorer,
                Question = "Who will be top scorer?",
                Points = 15,
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            },
            new BonusQuestion
            {
                Id = Guid.NewGuid(),
                TournamentId = otherTournamentId, // different tournament
                QuestionType = BonusQuestionType.Winner,
                Question = "Who will win the other tournament?",
                Points = 10,
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.BonusQuestions).Returns(bonusQuestions.BuildMockDbSet().Object);

        var handler = new GetBonusQuestionsByTournamentQueryHandler(dbMock.Object, mapper);

        var query = new GetBonusQuestionsByTournamentQuery(TournamentId: tournamentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
        result.Data.Should().AllSatisfy(bq => bq.TournamentId.Should().Be(tournamentId));
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_BonusQuestions_Exist()
    {
        // Arrange
        var mapper = CreateMapper();
        var tournamentId = Guid.NewGuid();

        var bonusQuestions = new List<BonusQuestion>();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.BonusQuestions).Returns(bonusQuestions.BuildMockDbSet().Object);

        var handler = new GetBonusQuestionsByTournamentQueryHandler(dbMock.Object, mapper);

        var query = new GetBonusQuestionsByTournamentQuery(TournamentId: tournamentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Order_By_QuestionType()
    {
        // Arrange
        var mapper = CreateMapper();
        var tournamentId = Guid.NewGuid();

        var bonusQuestions = new List<BonusQuestion>
        {
            new BonusQuestion
            {
                Id = Guid.NewGuid(),
                TournamentId = tournamentId,
                QuestionType = BonusQuestionType.TopScorer, // should be second (1)
                Question = "Top scorer?",
                Points = 15,
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            },
            new BonusQuestion
            {
                Id = Guid.NewGuid(),
                TournamentId = tournamentId,
                QuestionType = BonusQuestionType.Winner, // should be first (0)
                Question = "Winner?",
                Points = 10,
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.BonusQuestions).Returns(bonusQuestions.BuildMockDbSet().Object);

        var handler = new GetBonusQuestionsByTournamentQueryHandler(dbMock.Object, mapper);

        var query = new GetBonusQuestionsByTournamentQuery(TournamentId: tournamentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Should().HaveCount(2);
        result.Data[0].QuestionType.Should().Be(BonusQuestionType.Winner);
        result.Data[1].QuestionType.Should().Be(BonusQuestionType.TopScorer);
    }
}
