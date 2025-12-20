using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Predictions.Commands.UpdatePrediction;
using Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Predictions.Commands;

public sealed class UpdatePredictionCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Update_Prediction_When_Before_Deadline_And_User_Is_Owner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();
        var predictionId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            Settings = new LeagueSettings
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId,
                DeadlineMinutes = 60,
                AllowLateEdits = false,
                PointsCorrectScore = 7,
                PointsCorrectOutcome = 3,
                PointsCorrectGoals = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            CreatedAt = DateTime.UtcNow
        };

        // OBS: explicit Domain.Entities.Match för att undvika Moq.Match-krock
        var match = new Domain.Entities.Match
        {
            Id = matchId,
            MatchDate = DateTime.UtcNow.AddHours(2), // före deadline
            HomeScore = null,
            AwayScore = null
        };

        var prediction = new Prediction
        {
            Id = predictionId,
            UserId = userId,
            LeagueId = leagueId,
            MatchId = matchId,
            HomeScore = 0,
            AwayScore = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            PointsEarned = 0,
            IsScored = false
        };

        var leagues = new List<League> { league };
        var matches = new List<Domain.Entities.Match> { match };
        var predictions = new List<Prediction> { prediction };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leagues.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Matches).Returns(matches.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Predictions).Returns(predictions.BuildMockDbSet().Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var pointsMock = new Mock<IPointsCalculator>();

        var handler = new UpdatePredictionCommandHandler(dbMock.Object, currentUserMock.Object, pointsMock.Object);

        var cmd = new UpdatePredictionCommand(
            PredictionId: predictionId,
            HomeScore: 2,
            AwayScore: 1
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        prediction.HomeScore.Should().Be(2);
        prediction.AwayScore.Should().Be(1);
        prediction.UpdatedAt.Should().BeAfter(prediction.CreatedAt);

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Prediction_Not_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();
        var predictionId = Guid.NewGuid();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Predictions).Returns(new List<Prediction>().BuildMockDbSet().Object);
        dbMock.Setup(x => x.Matches).Returns(new List<Domain.Entities.Match>().BuildMockDbSet().Object);
        dbMock.Setup(x => x.Leagues).Returns(new List<League>().BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var pointsMock = new Mock<IPointsCalculator>();

        var handler = new UpdatePredictionCommandHandler(dbMock.Object, currentUserMock.Object, pointsMock.Object);

        var cmd = new UpdatePredictionCommand(
            PredictionId: predictionId,
            HomeScore: 1,
            AwayScore: 0
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error!.Code.Should().Be("prediction.not_found");

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_Not_Owner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();
        var predictionId = Guid.NewGuid();

        var prediction = new Prediction
        {
            Id = predictionId,
            UserId = otherUserId, // not current user
            LeagueId = leagueId,
            MatchId = matchId,
            HomeScore = 0,
            AwayScore = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Predictions).Returns(new List<Prediction> { prediction }.BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var pointsMock = new Mock<IPointsCalculator>();

        var handler = new UpdatePredictionCommandHandler(dbMock.Object, currentUserMock.Object, pointsMock.Object);

        var cmd = new UpdatePredictionCommand(
            PredictionId: predictionId,
            HomeScore: 2,
            AwayScore: 2
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        result.Error!.Code.Should().Be("prediction.forbidden");

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Deadline_Passed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();
        var predictionId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            Settings = new LeagueSettings
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId,
                DeadlineMinutes = 60,
                AllowLateEdits = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            CreatedAt = DateTime.UtcNow
        };

        var match = new Domain.Entities.Match
        {
            Id = matchId,
            MatchDate = DateTime.UtcNow.AddMinutes(-1) // match startade nyss => deadline passerad (60 min innan)
        };

        var prediction = new Prediction
        {
            Id = predictionId,
            UserId = userId,
            LeagueId = leagueId,
            MatchId = matchId,
            HomeScore = 0,
            AwayScore = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(new List<League> { league }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Matches).Returns(new List<Domain.Entities.Match> { match }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Predictions).Returns(new List<Prediction> { prediction }.BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var pointsMock = new Mock<IPointsCalculator>();

        var handler = new UpdatePredictionCommandHandler(dbMock.Object, currentUserMock.Object, pointsMock.Object);

        var cmd = new UpdatePredictionCommand(
            PredictionId: predictionId,
            HomeScore: 1,
            AwayScore: 0
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ErrorType.BusinessRule);
        result.Error!.Code.Should().Be("prediction.deadline_passed");

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Recalculate_Points_When_Match_Has_Score()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();
        var predictionId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            Settings = new LeagueSettings
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId,
                DeadlineMinutes = 60,
                AllowLateEdits = false,
                PointsCorrectScore = 7,
                PointsCorrectOutcome = 3,
                PointsCorrectGoals = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            CreatedAt = DateTime.UtcNow
        };

        var match = new Domain.Entities.Match
        {
            Id = matchId,
            MatchDate = DateTime.UtcNow.AddHours(2),
            HomeScore = 2,
            AwayScore = 1
        };

        var prediction = new Prediction
        {
            Id = predictionId,
            UserId = userId,
            LeagueId = leagueId,
            MatchId = matchId,
            HomeScore = 0,
            AwayScore = 0,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            PointsEarned = 0,
            IsScored = false
        };

        var pointsMock = new Mock<IPointsCalculator>();
        pointsMock
            .Setup(x => x.CalculateMatchPoints(2, 1, 2, 1, It.IsAny<LeagueSettings>()))
            .Returns(7);

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(new List<League> { league }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Matches).Returns(new List<Domain.Entities.Match> { match }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Predictions).Returns(new List<Prediction> { prediction }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new UpdatePredictionCommandHandler(dbMock.Object, currentUserMock.Object, pointsMock.Object);

        var cmd = new UpdatePredictionCommand(
            PredictionId: predictionId,
            HomeScore: 2,
            AwayScore: 1
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        prediction.PointsEarned.Should().Be(7);

        pointsMock.Verify(
            x => x.CalculateMatchPoints(2, 1, 2, 1, It.IsAny<LeagueSettings>()),
            Times.Once);

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
