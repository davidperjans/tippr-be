using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Predictions.DTOs;
using Application.Features.Predictions.Mapping;
using Application.Features.Predictions.Queries.GetUserPredictions;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Predictions.Queries;

public sealed class GetUserPredictionsQueryHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<PredictionProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_Return_Only_CurrentUsers_Predictions_For_League()
    {
        // Arrange
        var mapper = CreateMapper();

        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var predictions = new List<Prediction>
        {
            new Prediction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LeagueId = leagueId,
                MatchId = matchId,
                HomeScore = 2,
                AwayScore = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Prediction
            {
                Id = Guid.NewGuid(),
                UserId = otherUserId, // 다른 user
                LeagueId = leagueId,
                MatchId = matchId,
                HomeScore = 0,
                AwayScore = 0,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Predictions).Returns(predictions.BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new GetUserPredictionsQueryHandler(dbMock.Object, currentUserMock.Object, mapper);

        var query = new GetUserPredictionsQuery(LeagueId: leagueId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        result.Data!.Should().HaveCount(1);
        result.Data[0].LeagueId.Should().Be(leagueId);
        result.Data[0].MatchId.Should().Be(matchId);
        // viktigast:
        result.Data[0].UserId.Should().Be(userId);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_CurrentUser_Has_No_Predictions_In_League()
    {
        // Arrange
        var mapper = CreateMapper();

        var leagueId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var predictions = new List<Prediction>
        {
            new Prediction
            {
                Id = Guid.NewGuid(),
                UserId = otherUserId,
                LeagueId = leagueId,
                MatchId = Guid.NewGuid(),
                HomeScore = 1,
                AwayScore = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Predictions).Returns(predictions.BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new GetUserPredictionsQueryHandler(dbMock.Object, currentUserMock.Object, mapper);

        var query = new GetUserPredictionsQuery(leagueId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().BeEmpty();
    }
}
