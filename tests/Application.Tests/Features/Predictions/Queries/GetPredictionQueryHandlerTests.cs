using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.Mapping;
using Application.Features.Predictions.DTOs;
using Application.Features.Predictions.Mapping;
using Application.Features.Predictions.Queries.GetPrediction;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Predictions.Queries;

public sealed class GetPredictionQueryHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<PredictionProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_Return_Prediction_When_Found_For_Match_And_League_And_User()
    {
        // Arrange
        var mapper = CreateMapper();

        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        var prediction = new Prediction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LeagueId = leagueId,
            MatchId = matchId,
            HomeScore = 2,
            AwayScore = 1,
            PointsEarned = 7,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var predictions = new List<Prediction> { prediction };
        var predictionsDbSetMock = predictions.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Predictions).Returns(predictionsDbSetMock.Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new GetPredictionQueryHandler(dbMock.Object, currentUserMock.Object, mapper);

        var query = new GetPredictionQuery(
            MatchId: matchId,
            LeagueId: leagueId
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        result.Data!.MatchId.Should().Be(matchId);
        result.Data.LeagueId.Should().Be(leagueId);
        result.Data.HomeScore.Should().Be(2);
        result.Data.AwayScore.Should().Be(1);
        result.Data.PointsEarned.Should().Be(7);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Prediction_Does_Not_Exist()
    {
        // Arrange
        var mapper = CreateMapper();

        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        var predictions = new List<Prediction>(); // empty
        var predictionsDbSetMock = predictions.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Predictions).Returns(predictionsDbSetMock.Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new GetPredictionQueryHandler(dbMock.Object, currentUserMock.Object, mapper);

        var query = new GetPredictionQuery(matchId, leagueId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error!.Code.Should().Be("prediction.not_found");
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Prediction_Exists_But_For_Another_User()
    {
        // Arrange
        var mapper = CreateMapper();

        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        var prediction = new Prediction
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId, // different user
            LeagueId = leagueId,
            MatchId = matchId,
            HomeScore = 1,
            AwayScore = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var predictions = new List<Prediction> { prediction };
        var predictionsDbSetMock = predictions.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Predictions).Returns(predictionsDbSetMock.Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new GetPredictionQueryHandler(dbMock.Object, currentUserMock.Object, mapper);

        var query = new GetPredictionQuery(matchId, leagueId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        // Viktigt: vi läcker inte ut att andra användare har prediction för matchen
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error!.Code.Should().Be("prediction.not_found");
    }
}
