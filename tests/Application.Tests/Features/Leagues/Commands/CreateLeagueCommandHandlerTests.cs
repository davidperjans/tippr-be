using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.Commands.CreateLeague;
using Application.Features.Leagues.Mapping;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Leagues.Commands;

public sealed class CreateLeagueCommandHandlerTests
{
    #region Helper Methods

    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<LeagueProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    private static (CreateLeagueCommandHandler handler, Mock<ITipprDbContext> dbMock, List<League> leagues, List<LeagueMember> members, List<LeagueStanding> standings)
        CreateHandler(Guid currentUserId)
    {
        var leagues = new List<League>();
        var members = new List<LeagueMember>();
        var standings = new List<LeagueStanding>();

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var membersDbSetMock = members.BuildMockDbSet();
        var standingsDbSetMock = standings.BuildMockDbSet();

        leaguesDbSetMock.Setup(x => x.Add(It.IsAny<League>())).Callback<League>(leagues.Add);
        membersDbSetMock.Setup(x => x.Add(It.IsAny<LeagueMember>())).Callback<LeagueMember>(members.Add);
        standingsDbSetMock.Setup(x => x.Add(It.IsAny<LeagueStanding>())).Callback<LeagueStanding>(standings.Add);

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(membersDbSetMock.Object);
        dbMock.Setup(x => x.LeagueStandings).Returns(standingsDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(x => x.UserId).Returns(currentUserId);

        var standingsServiceMock = new Mock<IStandingsService>();

        var handler = new CreateLeagueCommandHandler(dbMock.Object, CreateMapper(), currentUserMock.Object, standingsServiceMock.Object);

        return (handler, dbMock, leagues, members, standings);
    }

    #endregion

    [Fact]
    public async Task Handle_Should_Create_League_And_Return_Id()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var (handler, dbMock, leagues, members, standings) = CreateHandler(ownerId);

        var cmd = new CreateLeagueCommand(
            Name: "My League",
            Description: "A test league",
            TournamentId: tournamentId,
            IsPublic: false,
            MaxMembers: 20,
            ImageUrl: "https://example.com/image.png"
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);

        var league = leagues.Single();
        league.Name.Should().Be("My League");
        league.Description.Should().Be("A test league");
        league.TournamentId.Should().Be(tournamentId);
        league.OwnerId.Should().Be(ownerId);
        league.IsPublic.Should().BeFalse();
        league.MaxMembers.Should().Be(20);
        league.InviteCode.Should().NotBeNullOrEmpty();
        league.InviteCode.Should().HaveLength(8);
        league.IsGlobal.Should().BeFalse();
        league.Settings.Should().NotBeNull();

        members.Should().ContainSingle(m => m.UserId == ownerId && m.IsAdmin);
        standings.Should().ContainSingle(s => s.UserId == ownerId && s.Rank == 1);

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Set_Owner_From_CurrentUser()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var (handler, _, leagues, members, standings) = CreateHandler(ownerId);

        var cmd = new CreateLeagueCommand(
            Name: "My League",
            Description: "Test",
            TournamentId: tournamentId,
            IsPublic: false,
            MaxMembers: 20,
            ImageUrl: null
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var league = leagues.Single();
        league.OwnerId.Should().Be(ownerId);
        league.Name.Should().Be("My League");
        league.InviteCode.Should().HaveLength(8);
        league.IsGlobal.Should().BeFalse();

        members.Should().ContainSingle(m => m.UserId == ownerId && m.IsAdmin);
        standings.Should().ContainSingle(s => s.UserId == ownerId && s.Rank == 1);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_League_With_Same_Name_And_Owner_Exists()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        var existingLeagues = new List<League>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "My League",
                OwnerId = ownerId,
                TournamentId = tournamentId,
                InviteCode = "ABC12345",
                CreatedAt = DateTime.UtcNow
            }
        };

        var leaguesDbSetMock = existingLeagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(x => x.UserId).Returns(ownerId);

        var standingsServiceMock = new Mock<IStandingsService>();

        var handler = new CreateLeagueCommandHandler(dbMock.Object, CreateMapper(), currentUserMock.Object, standingsServiceMock.Object);

        var cmd = new CreateLeagueCommand(
            Name: "My League",
            Description: "A test league",
            TournamentId: tournamentId,
            IsPublic: false,
            MaxMembers: null,
            ImageUrl: null
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("league already exists");
        result.Error.Code.Should().Be("league.already_exists");
        result.Error.Type.Should().Be(ErrorType.Conflict);

        leaguesDbSetMock.Verify(s => s.Add(It.IsAny<League>()), Times.Never);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Allow_Same_Name_With_Different_Owner()
    {
        // Arrange
        var existingOwnerId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        var existingLeagues = new List<League>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "My League",
                OwnerId = existingOwnerId,
                TournamentId = tournamentId,
                InviteCode = "ABC12345",
                CreatedAt = DateTime.UtcNow
            }
        };

        var members = new List<LeagueMember>();
        var standings = new List<LeagueStanding>();

        var leaguesDbSetMock = existingLeagues.BuildMockDbSet();
        var membersDbSetMock = members.BuildMockDbSet();
        var standingsDbSetMock = standings.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(membersDbSetMock.Object);
        dbMock.Setup(x => x.LeagueStandings).Returns(standingsDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(x => x.UserId).Returns(newOwnerId);

        var standingsServiceMock = new Mock<IStandingsService>();

        var handler = new CreateLeagueCommandHandler(dbMock.Object, CreateMapper(), currentUserMock.Object, standingsServiceMock.Object);

        var cmd = new CreateLeagueCommand(
            Name: "My League",
            Description: "A test league",
            TournamentId: tournamentId,
            IsPublic: false,
            MaxMembers: null,
            ImageUrl: null
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_Should_Create_Default_LeagueSettings()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var (handler, _, leagues, _, _) = CreateHandler(ownerId);

        var cmd = new CreateLeagueCommand(
            Name: "My League",
            Description: null,
            TournamentId: tournamentId,
            IsPublic: true,
            MaxMembers: null,
            ImageUrl: null
        );

        // Act
        await handler.Handle(cmd, CancellationToken.None);

        // Assert
        var league = leagues.Single();
        league.Settings.Should().NotBeNull();
        league.Settings!.DeadlineMinutes.Should().Be(60);
        league.Settings.PointsCorrectScore.Should().Be(7);
        league.Settings.PointsCorrectOutcome.Should().Be(3);
        league.Settings.PointsCorrectGoals.Should().Be(2);
        league.Settings.PointsRoundOf16Team.Should().Be(2);
        league.Settings.PointsQuarterFinalTeam.Should().Be(4);
        league.Settings.PointsSemiFinalTeam.Should().Be(6);
        league.Settings.PointsFinalTeam.Should().Be(8);
        league.Settings.PointsTopScorer.Should().Be(20);
        league.Settings.PointsWinner.Should().Be(20);
        league.Settings.AllowLateEdits.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Generate_Unique_8_Character_InviteCode()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var (handler, _, leagues, _, _) = CreateHandler(ownerId);

        var cmd = new CreateLeagueCommand(
            Name: "My League",
            Description: null,
            TournamentId: tournamentId,
            IsPublic: false,
            MaxMembers: null,
            ImageUrl: null
        );

        // Act
        await handler.Handle(cmd, CancellationToken.None);

        // Assert
        var league = leagues.Single();
        league.InviteCode.Should().NotBeNullOrEmpty();
        league.InviteCode.Should().HaveLength(8);
        league.InviteCode.Should().MatchRegex("^[A-Z0-9]+$");
    }
}
