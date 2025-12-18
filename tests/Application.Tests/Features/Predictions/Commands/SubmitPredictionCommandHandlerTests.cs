using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Predictions.Commands.SubmitPrediction;
using Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Predictions.Commands;

public sealed class SubmitPredictionCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Prediction_When_Before_Deadline()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            IsPublic = true,
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
            MatchDate = DateTime.UtcNow.AddHours(2), // före deadline
            HomeScore = null,
            AwayScore = null
        };

        var leagues = new List<League> { league };
        var matches = new List<Domain.Entities.Match> { match };
        var predictions = new List<Prediction>();

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var matchesDbSetMock = matches.BuildMockDbSet();
        var predictionsDbSetMock = predictions.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.Matches).Returns(matchesDbSetMock.Object);
        dbMock.Setup(x => x.Predictions).Returns(predictionsDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Viktigt: fånga Add så vi kan assert:a att entity skapades
        predictionsDbSetMock
            .Setup(x => x.Add(It.IsAny<Prediction>()))
            .Callback<Prediction>(p => predictions.Add(p));

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var pointsMock = new Mock<IPointsCalculator>();

        var handler = new SubmitPredictionCommandHandler(dbMock.Object, currentUserMock.Object, pointsMock.Object);

        var cmd = new SubmitPredictionCommand(
            LeagueId: leagueId,
            MatchId: matchId,
            HomeScore: 2,
            AwayScore: 1
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        predictions.Should().HaveCount(1);
        predictions[0].UserId.Should().Be(userId);
        predictions[0].LeagueId.Should().Be(leagueId);
        predictions[0].MatchId.Should().Be(matchId);
        predictions[0].HomeScore.Should().Be(2);
        predictions[0].AwayScore.Should().Be(1);

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_League_Not_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        var leagues = new List<League>(); // empty
        var matches = new List<Domain.Entities.Match> { new Domain.Entities.Match { Id = matchId, MatchDate = DateTime.UtcNow.AddHours(2) } };
        var predictions = new List<Prediction>();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leagues.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Matches).Returns(matches.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Predictions).Returns(predictions.BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var pointsMock = new Mock<IPointsCalculator>();

        var handler = new SubmitPredictionCommandHandler(dbMock.Object, currentUserMock.Object, pointsMock.Object);

        var cmd = new SubmitPredictionCommand(leagueId, matchId, 1, 0);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error!.Code.Should().Be("league.not_found");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Match_Not_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        var leagues = new List<League>
    {
        new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            Settings = new LeagueSettings { Id = Guid.NewGuid(), LeagueId = leagueId, DeadlineMinutes = 60, AllowLateEdits = false, CreatedAt = DateTime.UtcNow },
            CreatedAt = DateTime.UtcNow
        }
    };

        var matches = new List<Domain.Entities.Match>(); // empty
        var predictions = new List<Prediction>();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leagues.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Matches).Returns(matches.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Predictions).Returns(predictions.BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var pointsMock = new Mock<IPointsCalculator>();

        var handler = new SubmitPredictionCommandHandler(dbMock.Object, currentUserMock.Object, pointsMock.Object);

        var cmd = new SubmitPredictionCommand(leagueId, matchId, 1, 0);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error!.Code.Should().Be("match.not_found");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Prediction_Already_Exists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            Settings = new LeagueSettings { Id = Guid.NewGuid(), LeagueId = leagueId, DeadlineMinutes = 60, AllowLateEdits = false, CreatedAt = DateTime.UtcNow },
            CreatedAt = DateTime.UtcNow
        };

        var match = new Domain.Entities.Match { Id = matchId, MatchDate = DateTime.UtcNow.AddHours(2) };

        var predictions = new List<Prediction>
    {
        new Prediction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LeagueId = leagueId,
            MatchId = matchId,
            HomeScore = 1,
            AwayScore = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }
    };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(new List<League> { league }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Matches).Returns(new List<Domain.Entities.Match> { match }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Predictions).Returns(predictions.BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var pointsMock = new Mock<IPointsCalculator>();

        var handler = new SubmitPredictionCommandHandler(dbMock.Object, currentUserMock.Object, pointsMock.Object);

        var cmd = new SubmitPredictionCommand(leagueId, matchId, 2, 0);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error!.Code.Should().Be("prediction.already_exists");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Deadline_Passed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

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
                CreatedAt = DateTime.UtcNow
            },
            CreatedAt = DateTime.UtcNow
        };

        // Match startade för 1 minut sen -> deadline (60 min innan) passerad
        var match = new Domain.Entities.Match
        {
            Id = matchId,
            MatchDate = DateTime.UtcNow.AddMinutes(-1)
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(new List<League> { league }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Matches).Returns(new List<Domain.Entities.Match> { match }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Predictions).Returns(new List<Prediction>().BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var pointsMock = new Mock<IPointsCalculator>();

        var handler = new SubmitPredictionCommandHandler(dbMock.Object, currentUserMock.Object, pointsMock.Object);

        var cmd = new SubmitPredictionCommand(leagueId, matchId, 1, 0);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ErrorType.BusinessRule);
        result.Error!.Code.Should().Be("prediction.deadline_passed");
    }

    [Fact]
    public async Task Handle_Should_Set_PointsEarned_When_Match_Has_Score()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

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
                CreatedAt = DateTime.UtcNow
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

        var pointsMock = new Mock<IPointsCalculator>();
        pointsMock
            .Setup(x => x.CalculateMatchPoints(2, 1, 2, 1, It.IsAny<LeagueSettings>()))
            .Returns(7);

        var predictions = new List<Prediction>();
        var predictionsDbSetMock = predictions.BuildMockDbSet();
        predictionsDbSetMock
            .Setup(x => x.Add(It.IsAny<Prediction>()))
            .Callback<Prediction>(p => predictions.Add(p));

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(new List<League> { league }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Matches).Returns(new List<Domain.Entities.Match> { match }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.Predictions).Returns(predictionsDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new SubmitPredictionCommandHandler(dbMock.Object, currentUserMock.Object, pointsMock.Object);

        var cmd = new SubmitPredictionCommand(leagueId, matchId, 2, 1);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        predictions.Should().HaveCount(1);
        predictions[0].PointsEarned.Should().Be(7);

        pointsMock.Verify(
            x => x.CalculateMatchPoints(2, 1, 2, 1, It.IsAny<LeagueSettings>()),
            Times.Once);
    }
}
