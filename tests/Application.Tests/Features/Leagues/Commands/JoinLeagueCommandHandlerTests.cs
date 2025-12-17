using Application.Common.Interfaces;
using Application.Features.Leagues.Commands.JoinLeague;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Leagues.Commands;

public sealed class JoinLeagueCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Add_User_To_League_With_Valid_InviteCode()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();
        var inviteCode = "ABC12345";

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = ownerId,
            TournamentId = Guid.NewGuid(),
            InviteCode = inviteCode,
            IsPublic = false,
            MaxMembers = 10,
            Members = new List<LeagueMember>
            {
                new() { Id = Guid.NewGuid(), LeagueId = leagueId, UserId = ownerId, IsAdmin = true }
            },
            CreatedAt = DateTime.UtcNow
        };

        var leagues = new List<League> { league };
        var leagueMembers = new List<LeagueMember>(league.Members);
        var leagueStandings = new List<LeagueStanding>();

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();
        var leagueStandingsDbSetMock = leagueStandings.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);
        dbMock.Setup(x => x.LeagueStandings).Returns(leagueStandingsDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new JoinLeagueCommandHandler(dbMock.Object);

        var cmd = new JoinLeagueCommand(leagueId, newUserId, inviteCode);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        leagueMembersDbSetMock.Verify(s => s.Add(It.Is<LeagueMember>(m =>
            m.LeagueId == leagueId &&
            m.UserId == newUserId &&
            m.IsAdmin == false &&
            m.IsMuted == false
        )), Times.Once);

        leagueStandingsDbSetMock.Verify(s => s.Add(It.Is<LeagueStanding>(st =>
            st.LeagueId == leagueId &&
            st.UserId == newUserId &&
            st.TotalPoints == 0 &&
            st.Rank == 0
        )), Times.Once);

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_League_Not_Found()
    {
        // Arrange
        var nonExistentLeagueId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var leagues = new List<League>();
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new JoinLeagueCommandHandler(dbMock.Object);

        var cmd = new JoinLeagueCommand(nonExistentLeagueId, userId, "ABC12345");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("league not found");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_InviteCode_Is_Invalid_For_Private_League()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = ownerId,
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            IsPublic = false,
            Members = new List<LeagueMember>(),
            CreatedAt = DateTime.UtcNow
        };

        var leagues = new List<League> { league };
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new JoinLeagueCommandHandler(dbMock.Object);

        var cmd = new JoinLeagueCommand(leagueId, newUserId, "WRONGCODE");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("invalid invite code");
    }

    [Fact]
    public async Task Handle_Should_Allow_Join_Public_League_Without_Code()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Public League",
            OwnerId = ownerId,
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            IsPublic = true,
            MaxMembers = 100,
            Members = new List<LeagueMember>(),
            CreatedAt = DateTime.UtcNow
        };

        var leagues = new List<League> { league };
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

        var handler = new JoinLeagueCommandHandler(dbMock.Object);

        // Join with wrong code but public league should allow it
        var cmd = new JoinLeagueCommand(leagueId, newUserId, "ANYCODE");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        leagueMembersDbSetMock.Verify(s => s.Add(It.IsAny<LeagueMember>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_User_Already_Member()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var existingUserId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            IsPublic = true,
            Members = new List<LeagueMember>
            {
                new() { Id = Guid.NewGuid(), LeagueId = leagueId, UserId = existingUserId, IsAdmin = false }
            },
            CreatedAt = DateTime.UtcNow
        };

        var leagues = new List<League> { league };
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new JoinLeagueCommandHandler(dbMock.Object);

        var cmd = new JoinLeagueCommand(leagueId, existingUserId, "ABC12345");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        // Should not add member again
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_League_Is_Full()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();

        var members = Enumerable.Range(0, 5)
            .Select(_ => new LeagueMember
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId,
                UserId = Guid.NewGuid(),
                IsAdmin = false
            })
            .ToList();

        var league = new League
        {
            Id = leagueId,
            Name = "Full League",
            OwnerId = Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            IsPublic = true,
            MaxMembers = 5, // League is full
            Members = members,
            CreatedAt = DateTime.UtcNow
        };

        var leagues = new List<League> { league };
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new JoinLeagueCommandHandler(dbMock.Object);

        var cmd = new JoinLeagueCommand(leagueId, newUserId, "ABC12345");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("league is full");
    }

    [Fact]
    public async Task Handle_Should_Accept_InviteCode_Case_Insensitive()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            IsPublic = false,
            Members = new List<LeagueMember>(),
            CreatedAt = DateTime.UtcNow
        };

        var leagues = new List<League> { league };
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

        var handler = new JoinLeagueCommandHandler(dbMock.Object);

        // Lowercase invite code should work
        var cmd = new JoinLeagueCommand(leagueId, newUserId, "abc12345");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Not_Create_Duplicate_Standing()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            IsPublic = true,
            Members = new List<LeagueMember>(),
            CreatedAt = DateTime.UtcNow
        };

        var leagues = new List<League> { league };
        var leagueMembers = new List<LeagueMember>();

        // Standing already exists
        var leagueStandings = new List<LeagueStanding>
        {
            new() { Id = Guid.NewGuid(), LeagueId = leagueId, UserId = newUserId, TotalPoints = 0, Rank = 1 }
        };

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();
        var leagueStandingsDbSetMock = leagueStandings.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);
        dbMock.Setup(x => x.LeagueStandings).Returns(leagueStandingsDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new JoinLeagueCommandHandler(dbMock.Object);

        var cmd = new JoinLeagueCommand(leagueId, newUserId, "ABC12345");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Should not add duplicate standing
        leagueStandingsDbSetMock.Verify(s => s.Add(It.IsAny<LeagueStanding>()), Times.Never);
    }
}
