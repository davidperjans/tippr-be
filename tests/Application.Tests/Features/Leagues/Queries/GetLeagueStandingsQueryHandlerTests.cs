using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.Mapping;
using Application.Features.Leagues.Queries.GetLeagueStandings;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Leagues.Queries;

public sealed class GetLeagueStandingsQueryHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<LeagueStandingProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_Return_Standings_When_User_Is_Member()
    {
        // Arrange
        var mapper = CreateMapper();
        var leagueId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var user1 = new User { Id = userId, Username = "player1", Email = "player1@test.com" };
        var user2 = new User { Id = Guid.NewGuid(), Username = "player2", Email = "player2@test.com" };
        var user3 = new User { Id = Guid.NewGuid(), Username = "player3", Email = "player3@test.com" };

        var leagues = new List<League>
        {
            new()
            {
                Id = leagueId,
                Name = "Test League",
                OwnerId = userId,
                TournamentId = Guid.NewGuid(),
                InviteCode = "ABC12345",
                CreatedAt = DateTime.UtcNow
            }
        };

        var leagueMembers = new List<LeagueMember>
        {
            new() { Id = Guid.NewGuid(), LeagueId = leagueId, UserId = userId, JoinedAt = DateTime.UtcNow }
        };

        var standings = new List<LeagueStanding>
        {
            new()
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId,
                UserId = user2.Id,
                User = user2,
                TotalPoints = 50,
                MatchPoints = 40,
                BonusPoints = 10,
                Rank = 1,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId,
                UserId = userId,
                User = user1,
                TotalPoints = 45,
                MatchPoints = 35,
                BonusPoints = 10,
                Rank = 2,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId,
                UserId = user3.Id,
                User = user3,
                TotalPoints = 30,
                MatchPoints = 25,
                BonusPoints = 5,
                Rank = 3,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();
        var standingsDbSetMock = standings.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);
        dbMock.Setup(x => x.LeagueStandings).Returns(standingsDbSetMock.Object);

        var handler = new GetLeagueStandingsQueryHandler(dbMock.Object, mapper);
        var query = new GetLeagueStandingsQuery(leagueId, userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(3);

        // Should be ordered by rank
        result.Data[0].Rank.Should().Be(1);
        result.Data[0].Username.Should().Be("player2");
        result.Data[0].TotalPoints.Should().Be(50);

        result.Data[1].Rank.Should().Be(2);
        result.Data[1].Username.Should().Be("player1");
        result.Data[1].TotalPoints.Should().Be(45);

        result.Data[2].Rank.Should().Be(3);
        result.Data[2].Username.Should().Be("player3");
        result.Data[2].TotalPoints.Should().Be(30);
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

        var handler = new GetLeagueStandingsQueryHandler(dbMock.Object, mapper);
        var query = new GetLeagueStandingsQuery(nonExistentLeagueId, userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("league not found.");
        result.Error!.Code.Should().Be("league.not_found");
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_Not_Member()
    {
        // Arrange
        var mapper = CreateMapper();
        var leagueId = Guid.NewGuid();
        var nonMemberUserId = Guid.NewGuid();
        var memberUserId = Guid.NewGuid();

        var leagues = new List<League>
        {
            new()
            {
                Id = leagueId,
                Name = "Test League",
                OwnerId = memberUserId,
                TournamentId = Guid.NewGuid(),
                InviteCode = "ABC12345",
                CreatedAt = DateTime.UtcNow
            }
        };

        var leagueMembers = new List<LeagueMember>
        {
            new() { Id = Guid.NewGuid(), LeagueId = leagueId, UserId = memberUserId, JoinedAt = DateTime.UtcNow }
        };

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);

        var handler = new GetLeagueStandingsQueryHandler(dbMock.Object, mapper);
        var query = new GetLeagueStandingsQuery(leagueId, nonMemberUserId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("not a member of this league.");
        result.Error!.Code.Should().Be("league.forbidden");
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Standings()
    {
        // Arrange
        var mapper = CreateMapper();
        var leagueId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var leagues = new List<League>
        {
            new()
            {
                Id = leagueId,
                Name = "New League",
                OwnerId = userId,
                TournamentId = Guid.NewGuid(),
                InviteCode = "ABC12345",
                CreatedAt = DateTime.UtcNow
            }
        };

        var leagueMembers = new List<LeagueMember>
        {
            new() { Id = Guid.NewGuid(), LeagueId = leagueId, UserId = userId, JoinedAt = DateTime.UtcNow }
        };

        var standings = new List<LeagueStanding>();

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();
        var standingsDbSetMock = standings.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);
        dbMock.Setup(x => x.LeagueStandings).Returns(standingsDbSetMock.Object);

        var handler = new GetLeagueStandingsQueryHandler(dbMock.Object, mapper);
        var query = new GetLeagueStandingsQuery(leagueId, userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Include_All_Standing_Properties()
    {
        // Arrange
        var mapper = CreateMapper();
        var leagueId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var user = new User { Id = userId, Username = "testuser", Email = "test@test.com" };

        var leagues = new List<League>
        {
            new()
            {
                Id = leagueId,
                Name = "Test League",
                OwnerId = userId,
                TournamentId = Guid.NewGuid(),
                InviteCode = "ABC12345",
                CreatedAt = DateTime.UtcNow
            }
        };

        var leagueMembers = new List<LeagueMember>
        {
            new() { Id = Guid.NewGuid(), LeagueId = leagueId, UserId = userId, JoinedAt = DateTime.UtcNow }
        };

        var updatedAt = DateTime.UtcNow;
        var standings = new List<LeagueStanding>
        {
            new()
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId,
                UserId = userId,
                User = user,
                TotalPoints = 100,
                MatchPoints = 80,
                BonusPoints = 20,
                Rank = 1,
                PreviousRank = 3,
                UpdatedAt = updatedAt
            }
        };

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();
        var standingsDbSetMock = standings.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);
        dbMock.Setup(x => x.LeagueStandings).Returns(standingsDbSetMock.Object);

        var handler = new GetLeagueStandingsQueryHandler(dbMock.Object, mapper);
        var query = new GetLeagueStandingsQuery(leagueId, userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);

        var standing = result.Data[0];
        standing.UserId.Should().Be(userId);
        standing.Username.Should().Be("testuser");
        standing.TotalPoints.Should().Be(100);
        standing.MatchPoints.Should().Be(80);
        standing.BonusPoints.Should().Be(20);
        standing.Rank.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Only_Return_Standings_For_Specified_League()
    {
        // Arrange
        var mapper = CreateMapper();
        var leagueId1 = Guid.NewGuid();
        var leagueId2 = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var user = new User { Id = userId, Username = "player", Email = "player@test.com" };

        var leagues = new List<League>
        {
            new()
            {
                Id = leagueId1,
                Name = "League 1",
                OwnerId = userId,
                TournamentId = Guid.NewGuid(),
                InviteCode = "ABC12345",
                CreatedAt = DateTime.UtcNow
            }
        };

        var leagueMembers = new List<LeagueMember>
        {
            new() { Id = Guid.NewGuid(), LeagueId = leagueId1, UserId = userId, JoinedAt = DateTime.UtcNow }
        };

        var standings = new List<LeagueStanding>
        {
            new()
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId1,
                UserId = userId,
                User = user,
                TotalPoints = 50,
                Rank = 1,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId2, // Different league
                UserId = userId,
                User = user,
                TotalPoints = 100,
                Rank = 1,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();
        var standingsDbSetMock = standings.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);
        dbMock.Setup(x => x.LeagueStandings).Returns(standingsDbSetMock.Object);

        var handler = new GetLeagueStandingsQueryHandler(dbMock.Object, mapper);
        var query = new GetLeagueStandingsQuery(leagueId1, userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data[0].TotalPoints.Should().Be(50);
    }
}
