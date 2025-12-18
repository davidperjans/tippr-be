using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.Commands.UpdateLeagueSettings;
using Application.Features.Leagues.Mapping;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Leagues.Commands;

public sealed class UpdateLeagueSettingsCommandHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<LeagueSettingsProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_Update_Settings_When_User_Is_Owner()
    {
        // Arrange
        var mapper = CreateMapper();
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var settingsId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = ownerId,
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            Settings = new LeagueSettings
            {
                Id = settingsId,
                LeagueId = leagueId,
                PredictionMode = PredictionMode.AllAtOnce,
                DeadlineMinutes = 60,
                PointsCorrectScore = 7,
                PointsCorrectOutcome = 3,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            CreatedAt = DateTime.UtcNow
        };

        var leagues = new List<League> { league };
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateLeagueSettingsCommandHandler(dbMock.Object, mapper);

        var cmd = new UpdateLeagueSettingsCommand(
            LeagueId: leagueId,
            UserId: ownerId,
            PredictionMode: PredictionMode.MatchByMatch,
            DeadlineMinutes: 30,
            PointsCorrectScore: 10,
            PointsCorrectOutcome: 5,
            PointsCorrectGoals: 3,
            PointsRoundOf16Team: 3,
            PointsQuarterFinalTeam: 5,
            PointsSemiFinalTeam: 7,
            PointsFinalTeam: 10,
            PointsTopScorer: 25,
            PointsWinner: 30,
            PointsMostGoalsGroup: 15,
            PointsMostConcededGroup: 15,
            AllowLateEdits: true
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PredictionMode.Should().Be("MatchByMatch");
        result.Data.DeadlineMinutes.Should().Be(30);
        result.Data.PointsCorrectScore.Should().Be(10);
        result.Data.PointsCorrectOutcome.Should().Be(5);
        result.Data.AllowLateEdits.Should().BeTrue();

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_League_Not_Found()
    {
        // Arrange
        var mapper = CreateMapper();
        var nonExistentLeagueId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var leagues = new List<League>();
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new UpdateLeagueSettingsCommandHandler(dbMock.Object, mapper);

        var cmd = new UpdateLeagueSettingsCommand(
            LeagueId: nonExistentLeagueId,
            UserId: userId,
            PredictionMode: PredictionMode.AllAtOnce,
            DeadlineMinutes: 60,
            PointsCorrectScore: 7,
            PointsCorrectOutcome: 3,
            PointsCorrectGoals: 2,
            PointsRoundOf16Team: 2,
            PointsQuarterFinalTeam: 4,
            PointsSemiFinalTeam: 6,
            PointsFinalTeam: 8,
            PointsTopScorer: 20,
            PointsWinner: 20,
            PointsMostGoalsGroup: 10,
            PointsMostConcededGroup: 10,
            AllowLateEdits: false
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("league not found.");
        result.Error!.Code.Should().Be("league.not_found");
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_Not_Owner()
    {
        // Arrange
        var mapper = CreateMapper();
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = ownerId,
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            Settings = new LeagueSettings
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId,
                PredictionMode = PredictionMode.AllAtOnce,
                CreatedAt = DateTime.UtcNow
            },
            CreatedAt = DateTime.UtcNow
        };

        var leagues = new List<League> { league };
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new UpdateLeagueSettingsCommandHandler(dbMock.Object, mapper);

        var cmd = new UpdateLeagueSettingsCommand(
            LeagueId: leagueId,
            UserId: differentUserId, // Not the owner
            PredictionMode: PredictionMode.AllAtOnce,
            DeadlineMinutes: 60,
            PointsCorrectScore: 7,
            PointsCorrectOutcome: 3,
            PointsCorrectGoals: 2,
            PointsRoundOf16Team: 2,
            PointsQuarterFinalTeam: 4,
            PointsSemiFinalTeam: 6,
            PointsFinalTeam: 8,
            PointsTopScorer: 20,
            PointsWinner: 20,
            PointsMostGoalsGroup: 10,
            PointsMostConcededGroup: 10,
            AllowLateEdits: false
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("only the league owner can update settings.");
        result.Error!.Code.Should().Be("league.forbidden");
        result.Error!.Type.Should().Be(ErrorType.Forbidden);

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Create_Settings_If_Not_Exists()
    {
        // Arrange
        var mapper = CreateMapper();
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = ownerId,
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            Settings = null, // No settings
            CreatedAt = DateTime.UtcNow
        };

        var leagues = new List<League> { league };
        var leagueSettings = new List<LeagueSettings>();

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var leagueSettingsDbSetMock = leagueSettings.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueSettings).Returns(leagueSettingsDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateLeagueSettingsCommandHandler(dbMock.Object, mapper);

        var cmd = new UpdateLeagueSettingsCommand(
            LeagueId: leagueId,
            UserId: ownerId,
            PredictionMode: PredictionMode.StageByStage,
            DeadlineMinutes: 45,
            PointsCorrectScore: 8,
            PointsCorrectOutcome: 4,
            PointsCorrectGoals: 2,
            PointsRoundOf16Team: 2,
            PointsQuarterFinalTeam: 4,
            PointsSemiFinalTeam: 6,
            PointsFinalTeam: 8,
            PointsTopScorer: 20,
            PointsWinner: 20,
            PointsMostGoalsGroup: 10,
            PointsMostConcededGroup: 10,
            AllowLateEdits: false
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        leagueSettingsDbSetMock.Verify(s => s.Add(It.IsAny<LeagueSettings>()), Times.Once);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var mapper = CreateMapper();
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var originalUpdatedAt = DateTime.UtcNow.AddDays(-1);

        var settings = new LeagueSettings
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            PredictionMode = PredictionMode.AllAtOnce,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = originalUpdatedAt
        };

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = ownerId,
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            Settings = settings,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = originalUpdatedAt
        };

        var leagues = new List<League> { league };
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateLeagueSettingsCommandHandler(dbMock.Object, mapper);

        var cmd = new UpdateLeagueSettingsCommand(
            LeagueId: leagueId,
            UserId: ownerId,
            PredictionMode: PredictionMode.AllAtOnce,
            DeadlineMinutes: 60,
            PointsCorrectScore: 7,
            PointsCorrectOutcome: 3,
            PointsCorrectGoals: 2,
            PointsRoundOf16Team: 2,
            PointsQuarterFinalTeam: 4,
            PointsSemiFinalTeam: 6,
            PointsFinalTeam: 8,
            PointsTopScorer: 20,
            PointsWinner: 20,
            PointsMostGoalsGroup: 10,
            PointsMostConcededGroup: 10,
            AllowLateEdits: false
        );

        // Act
        await handler.Handle(cmd, CancellationToken.None);

        // Assert
        settings.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        league.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData(PredictionMode.AllAtOnce)]
    [InlineData(PredictionMode.StageByStage)]
    [InlineData(PredictionMode.MatchByMatch)]
    public async Task Handle_Should_Map_All_PredictionModes_Correctly(PredictionMode mode)
    {
        // Arrange
        var mapper = CreateMapper();
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = ownerId,
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            Settings = new LeagueSettings
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId,
                PredictionMode = PredictionMode.AllAtOnce,
                CreatedAt = DateTime.UtcNow
            },
            CreatedAt = DateTime.UtcNow
        };

        var leagues = new List<League> { league };
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateLeagueSettingsCommandHandler(dbMock.Object, mapper);

        var cmd = new UpdateLeagueSettingsCommand(
            LeagueId: leagueId,
            UserId: ownerId,
            PredictionMode: mode,
            DeadlineMinutes: 60,
            PointsCorrectScore: 7,
            PointsCorrectOutcome: 3,
            PointsCorrectGoals: 2,
            PointsRoundOf16Team: 2,
            PointsQuarterFinalTeam: 4,
            PointsSemiFinalTeam: 6,
            PointsFinalTeam: 8,
            PointsTopScorer: 20,
            PointsWinner: 20,
            PointsMostGoalsGroup: 10,
            PointsMostConcededGroup: 10,
            AllowLateEdits: false
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.PredictionMode.Should().Be(mode.ToString());
    }
}
