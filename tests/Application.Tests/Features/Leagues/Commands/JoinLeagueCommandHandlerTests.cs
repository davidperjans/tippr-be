using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.Commands.JoinLeague;
using Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Leagues.Commands;

public sealed class JoinLeagueCommandHandlerTests
{
    #region Helper Methods

    private static (JoinLeagueCommandHandler handler, Mock<ITipprDbContext> dbMock, Mock<Microsoft.EntityFrameworkCore.DbSet<LeagueMember>> membersDbSetMock, Mock<Microsoft.EntityFrameworkCore.DbSet<LeagueStanding>> standingsDbSetMock)
        CreateHandler(
            Guid currentUserId,
            List<League> leagues,
            List<LeagueMember>? members = null,
            List<LeagueStanding>? standings = null)
    {
        members ??= new List<LeagueMember>();
        standings ??= new List<LeagueStanding>();

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var membersDbSetMock = members.BuildMockDbSet();
        var standingsDbSetMock = standings.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(membersDbSetMock.Object);
        dbMock.Setup(x => x.LeagueStandings).Returns(standingsDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(x => x.UserId).Returns(currentUserId);

        var standingsServiceMock = new Mock<IStandingsService>();

        var handler = new JoinLeagueCommandHandler(dbMock.Object, currentUserMock.Object, standingsServiceMock.Object);

        return (handler, dbMock, membersDbSetMock, standingsDbSetMock);
    }

    private static League CreateLeague(
        Guid? id = null,
        string name = "Test League",
        Guid? ownerId = null,
        string inviteCode = "ABC12345",
        bool isPublic = false,
        bool isGlobal = false,
        int? maxMembers = null,
        List<LeagueMember>? members = null)
    {
        return new League
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            OwnerId = ownerId ?? Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            InviteCode = inviteCode,
            IsPublic = isPublic,
            IsGlobal = isGlobal,
            MaxMembers = maxMembers,
            Members = members ?? new List<LeagueMember>(),
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion

    [Fact]
    public async Task Handle_Should_Add_User_To_League_With_Valid_InviteCode()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();
        var inviteCode = "ABC12345";

        var league = CreateLeague(
            id: leagueId,
            ownerId: ownerId,
            inviteCode: inviteCode,
            isPublic: false,
            maxMembers: 10,
            members: new List<LeagueMember>
            {
                new() { Id = Guid.NewGuid(), LeagueId = leagueId, UserId = ownerId, IsAdmin = true }
            });

        var leagues = new List<League> { league };
        var members = new List<LeagueMember>(league.Members);
        var standings = new List<LeagueStanding>();

        var (handler, dbMock, membersDbSetMock, standingsDbSetMock) = CreateHandler(newUserId, leagues, members, standings);

        var cmd = new JoinLeagueCommand(leagueId, inviteCode);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        membersDbSetMock.Verify(s => s.Add(It.Is<LeagueMember>(m =>
            m.LeagueId == leagueId &&
            m.UserId == newUserId &&
            m.IsAdmin == false &&
            m.IsMuted == false
        )), Times.Once);

        standingsDbSetMock.Verify(s => s.Add(It.Is<LeagueStanding>(st =>
            st.LeagueId == leagueId &&
            st.UserId == newUserId &&
            st.TotalPoints == 0 &&
            st.Rank == 1
        )), Times.Once);

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_League_Not_Found()
    {
        // Arrange
        var nonExistentLeagueId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var (handler, _, _, _) = CreateHandler(userId, new List<League>());

        var cmd = new JoinLeagueCommand(nonExistentLeagueId, "ABC12345");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("league not found");
        result.Error.Code.Should().Be("league.not_found");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_InviteCode_Is_Invalid_For_Private_League()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();

        var league = CreateLeague(id: leagueId, inviteCode: "ABC12345", isPublic: false);
        var leagues = new List<League> { league };

        var (handler, _, _, _) = CreateHandler(newUserId, leagues);

        var cmd = new JoinLeagueCommand(leagueId, "WRONGCODE");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("invalid invite code");
        result.Error.Code.Should().Be("league.invalid_invite_code");
        result.Error.Type.Should().Be(ErrorType.BusinessRule);
    }

    [Fact]
    public async Task Handle_Should_Allow_Join_Public_League_Without_Code()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();

        var league = CreateLeague(id: leagueId, isPublic: true, maxMembers: 100);
        var leagues = new List<League> { league };
        var members = new List<LeagueMember>();
        var standings = new List<LeagueStanding>();

        var (handler, _, membersDbSetMock, _) = CreateHandler(newUserId, leagues, members, standings);

        // Join with wrong code but public league should allow it
        var cmd = new JoinLeagueCommand(leagueId, "ANYCODE");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        membersDbSetMock.Verify(s => s.Add(It.IsAny<LeagueMember>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_User_Already_Member()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var existingUserId = Guid.NewGuid();

        var league = CreateLeague(
            id: leagueId,
            isPublic: true,
            members: new List<LeagueMember>
            {
                new() { Id = Guid.NewGuid(), LeagueId = leagueId, UserId = existingUserId, IsAdmin = false }
            });

        var leagues = new List<League> { league };
        var members = new List<LeagueMember>(league.Members);

        var (handler, dbMock, _, _) = CreateHandler(existingUserId, leagues, members);

        var cmd = new JoinLeagueCommand(leagueId, "ABC12345");

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

        var existingMembers = Enumerable.Range(0, 5)
            .Select(_ => new LeagueMember
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId,
                UserId = Guid.NewGuid(),
                IsAdmin = false
            })
            .ToList();

        var league = CreateLeague(id: leagueId, isPublic: true, maxMembers: 5, members: existingMembers);
        var leagues = new List<League> { league };
        var members = new List<LeagueMember>(existingMembers);

        var (handler, _, _, _) = CreateHandler(newUserId, leagues, members);

        var cmd = new JoinLeagueCommand(leagueId, "ABC12345");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("league is full");
        result.Error.Code.Should().Be("league.full");
        result.Error.Type.Should().Be(ErrorType.BusinessRule);
    }

    [Fact]
    public async Task Handle_Should_Accept_InviteCode_Case_Insensitive()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();

        var league = CreateLeague(id: leagueId, inviteCode: "ABC12345", isPublic: false);
        var leagues = new List<League> { league };
        var members = new List<LeagueMember>();
        var standings = new List<LeagueStanding>();

        var (handler, _, _, _) = CreateHandler(newUserId, leagues, members, standings);

        // Lowercase invite code should work
        var cmd = new JoinLeagueCommand(leagueId, "abc12345");

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

        var league = CreateLeague(id: leagueId, isPublic: true);
        var leagues = new List<League> { league };
        var members = new List<LeagueMember>();

        // Standing already exists for this user
        var standings = new List<LeagueStanding>
        {
            new() { Id = Guid.NewGuid(), LeagueId = leagueId, UserId = newUserId, TotalPoints = 0, Rank = 1 }
        };

        var (handler, _, _, standingsDbSetMock) = CreateHandler(newUserId, leagues, members, standings);

        var cmd = new JoinLeagueCommand(leagueId, "ABC12345");

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Should not add duplicate standing
        standingsDbSetMock.Verify(s => s.Add(It.IsAny<LeagueStanding>()), Times.Never);
    }
}
