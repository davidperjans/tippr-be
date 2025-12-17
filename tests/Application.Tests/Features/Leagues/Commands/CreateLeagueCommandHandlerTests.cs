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
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<LeagueProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_Create_League_And_Return_Id()
    {
        // Arrange
        var mapper = CreateMapper();
        var ownerId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        var leagues = new List<League>();
        var leagueMembers = new List<LeagueMember>();
        var leagueStandings = new List<LeagueStanding>();
        var leagueSettings = new List<LeagueSettings>();

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();
        var leagueStandingsDbSetMock = leagueStandings.BuildMockDbSet();
        var leagueSettingsDbSetMock = leagueSettings.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);
        dbMock.Setup(x => x.LeagueStandings).Returns(leagueStandingsDbSetMock.Object);
        dbMock.Setup(x => x.LeagueSettings).Returns(leagueSettingsDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateLeagueCommandHandler(dbMock.Object, mapper);

        var cmd = new CreateLeagueCommand(
            Name: "My League",
            Description: "A test league",
            TournamentId: tournamentId,
            OwnerId: ownerId,
            IsPublic: false,
            MaxMembers: 20,
            ImageUrl: "https://example.com/image.png"
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);

        // Verify league was added
        leaguesDbSetMock.Verify(s => s.Add(It.Is<League>(l =>
            l.Name == "My League" &&
            l.Description == "A test league" &&
            l.TournamentId == tournamentId &&
            l.OwnerId == ownerId &&
            l.IsPublic == false &&
            l.MaxMembers == 20 &&
            l.InviteCode != null &&
            l.InviteCode.Length == 8 &&
            l.IsGlobal == false &&
            l.Settings != null
        )), Times.Once);

        // Verify owner was added as admin member
        leagueMembersDbSetMock.Verify(s => s.Add(It.Is<LeagueMember>(m =>
            m.UserId == ownerId &&
            m.IsAdmin == true &&
            m.IsMuted == false
        )), Times.Once);

        // Verify standing was created for owner
        leagueStandingsDbSetMock.Verify(s => s.Add(It.Is<LeagueStanding>(st =>
            st.UserId == ownerId &&
            st.TotalPoints == 0 &&
            st.Rank == 1
        )), Times.Once);

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_League_With_Same_Name_And_Owner_Exists()
    {
        // Arrange
        var mapper = CreateMapper();
        var ownerId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        var leagues = new List<League>
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

        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new CreateLeagueCommandHandler(dbMock.Object, mapper);

        var cmd = new CreateLeagueCommand(
            Name: "My League",
            Description: "A test league",
            TournamentId: tournamentId,
            OwnerId: ownerId,
            IsPublic: false,
            MaxMembers: null,
            ImageUrl: null
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("league already exists");

        leaguesDbSetMock.Verify(s => s.Add(It.IsAny<League>()), Times.Never);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Allow_Same_Name_With_Different_Owner()
    {
        // Arrange
        var mapper = CreateMapper();
        var ownerId1 = Guid.NewGuid();
        var ownerId2 = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        var leagues = new List<League>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "My League",
                OwnerId = ownerId1,
                TournamentId = tournamentId,
                InviteCode = "ABC12345",
                CreatedAt = DateTime.UtcNow
            }
        };

        var leagueMembers = new List<LeagueMember>();
        var leagueStandings = new List<LeagueStanding>();

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();
        var leagueStandingsDbSetMock = leagueStandings.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);
        dbMock.Setup(x => x.LeagueStandings).Returns(leagueStandingsDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateLeagueCommandHandler(dbMock.Object, mapper);

        var cmd = new CreateLeagueCommand(
            Name: "My League",
            Description: "A test league",
            TournamentId: tournamentId,
            OwnerId: ownerId2, // Different owner
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
        var mapper = CreateMapper();
        var ownerId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        var leagues = new List<League>();
        var leagueMembers = new List<LeagueMember>();
        var leagueStandings = new List<LeagueStanding>();

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();
        var leagueStandingsDbSetMock = leagueStandings.BuildMockDbSet();

        League? addedLeague = null;
        leaguesDbSetMock.Setup(s => s.Add(It.IsAny<League>()))
            .Callback<League>(l => addedLeague = l);

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);
        dbMock.Setup(x => x.LeagueStandings).Returns(leagueStandingsDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateLeagueCommandHandler(dbMock.Object, mapper);

        var cmd = new CreateLeagueCommand(
            Name: "My League",
            Description: null,
            TournamentId: tournamentId,
            OwnerId: ownerId,
            IsPublic: true,
            MaxMembers: null,
            ImageUrl: null
        );

        // Act
        await handler.Handle(cmd, CancellationToken.None);

        // Assert
        addedLeague.Should().NotBeNull();
        addedLeague!.Settings.Should().NotBeNull();
        addedLeague.Settings!.DeadlineMinutes.Should().Be(60);
        addedLeague.Settings.PointsCorrectScore.Should().Be(7);
        addedLeague.Settings.PointsCorrectOutcome.Should().Be(3);
        addedLeague.Settings.PointsCorrectGoals.Should().Be(2);
        addedLeague.Settings.PointsRoundOf16Team.Should().Be(2);
        addedLeague.Settings.PointsQuarterFinalTeam.Should().Be(4);
        addedLeague.Settings.PointsSemiFinalTeam.Should().Be(6);
        addedLeague.Settings.PointsFinalTeam.Should().Be(8);
        addedLeague.Settings.PointsTopScorer.Should().Be(20);
        addedLeague.Settings.PointsWinner.Should().Be(20);
        addedLeague.Settings.AllowLateEdits.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Generate_Unique_8_Character_InviteCode()
    {
        // Arrange
        var mapper = CreateMapper();
        var ownerId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        var leagues = new List<League>();
        var leagueMembers = new List<LeagueMember>();
        var leagueStandings = new List<LeagueStanding>();

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();
        var leagueStandingsDbSetMock = leagueStandings.BuildMockDbSet();

        League? addedLeague = null;
        leaguesDbSetMock.Setup(s => s.Add(It.IsAny<League>()))
            .Callback<League>(l => addedLeague = l);

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);
        dbMock.Setup(x => x.LeagueStandings).Returns(leagueStandingsDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateLeagueCommandHandler(dbMock.Object, mapper);

        var cmd = new CreateLeagueCommand(
            Name: "My League",
            Description: null,
            TournamentId: tournamentId,
            OwnerId: ownerId,
            IsPublic: false,
            MaxMembers: null,
            ImageUrl: null
        );

        // Act
        await handler.Handle(cmd, CancellationToken.None);

        // Assert
        addedLeague.Should().NotBeNull();
        addedLeague!.InviteCode.Should().NotBeNullOrEmpty();
        addedLeague.InviteCode.Should().HaveLength(8);
        addedLeague.InviteCode.Should().MatchRegex("^[A-Z0-9]+$");
    }
}
