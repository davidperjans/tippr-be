using Application.Common.Interfaces;
using Application.Features.BonusQuestions.Queries.GetUserBonusPredictions;
using Application.Features.Predictions.Mapping;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.BonusQuestions.Queries;

public sealed class GetUserBonusPredictionsQueryHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<BonusPredictionProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_Return_Only_CurrentUsers_Predictions_For_League()
    {
        // Arrange
        var mapper = CreateMapper();

        var leagueId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var bonusQuestionId = Guid.NewGuid();

        var bonusPredictions = new List<BonusPrediction>
        {
            new BonusPrediction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LeagueId = leagueId,
                BonusQuestionId = bonusQuestionId,
                AnswerTeamId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            },
            new BonusPrediction
            {
                Id = Guid.NewGuid(),
                UserId = otherUserId, // different user
                LeagueId = leagueId,
                BonusQuestionId = bonusQuestionId,
                AnswerTeamId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            }
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.BonusPredictions).Returns(bonusPredictions.BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new GetUserBonusPredictionsQueryHandler(dbMock.Object, currentUserMock.Object, mapper);

        var query = new GetUserBonusPredictionsQuery(LeagueId: leagueId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
        result.Data[0].UserId.Should().Be(userId);
        result.Data[0].LeagueId.Should().Be(leagueId);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_User_Has_No_Predictions()
    {
        // Arrange
        var mapper = CreateMapper();

        var leagueId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var bonusPredictions = new List<BonusPrediction>
        {
            new BonusPrediction
            {
                Id = Guid.NewGuid(),
                UserId = otherUserId, // different user
                LeagueId = leagueId,
                BonusQuestionId = Guid.NewGuid(),
                AnswerTeamId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            }
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.BonusPredictions).Returns(bonusPredictions.BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new GetUserBonusPredictionsQueryHandler(dbMock.Object, currentUserMock.Object, mapper);

        var query = new GetUserBonusPredictionsQuery(LeagueId: leagueId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Only_Return_Predictions_For_Specified_League()
    {
        // Arrange
        var mapper = CreateMapper();

        var leagueId = Guid.NewGuid();
        var otherLeagueId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var bonusPredictions = new List<BonusPrediction>
        {
            new BonusPrediction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LeagueId = leagueId,
                BonusQuestionId = Guid.NewGuid(),
                AnswerTeamId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            },
            new BonusPrediction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LeagueId = otherLeagueId, // different league
                BonusQuestionId = Guid.NewGuid(),
                AnswerTeamId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            }
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.BonusPredictions).Returns(bonusPredictions.BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new GetUserBonusPredictionsQueryHandler(dbMock.Object, currentUserMock.Object, mapper);

        var query = new GetUserBonusPredictionsQuery(LeagueId: leagueId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
        result.Data[0].LeagueId.Should().Be(leagueId);
    }
}
