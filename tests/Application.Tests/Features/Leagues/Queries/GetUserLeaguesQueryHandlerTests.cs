using Application.Common.Interfaces;
using Application.Features.Leagues.Mapping;
using Application.Features.Leagues.Queries.GetUserLeagues;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Leagues.Queries;

public sealed class GetUserLeaguesQueryHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<LeagueProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_Return_Leagues_For_User()
    {
        // Arrange
        var mapper = CreateMapper();
        var userId = Guid.NewGuid();
        var leagueId1 = Guid.NewGuid();
        var leagueId2 = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        var league1 = new League
        {
            Id = leagueId1,
            Name = "League 1",
            Description = "First league",
            TournamentId = tournamentId,
            OwnerId = userId,
            InviteCode = "ABC12345",
            IsPublic = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var league2 = new League
        {
            Id = leagueId2,
            Name = "League 2",
            Description = "Second league",
            TournamentId = tournamentId,
            OwnerId = Guid.NewGuid(),
            InviteCode = "DEF67890",
            IsPublic = false,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        var leagueMembers = new List<LeagueMember>
        {
            new()
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId1,
                UserId = userId,
                League = league1,
                JoinedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                LeagueId = leagueId2,
                UserId = userId,
                League = league2,
                JoinedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);

        var handler = new GetUserLeaguesQueryHandler(dbMock.Object, mapper);
        var query = new GetUserLeaguesQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().Contain(l => l.Name == "League 1");
        result.Data.Should().Contain(l => l.Name == "League 2");
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_User_Has_No_Leagues()
    {
        // Arrange
        var mapper = CreateMapper();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var league = new League
        {
            Id = Guid.NewGuid(),
            Name = "Other User League",
            TournamentId = Guid.NewGuid(),
            OwnerId = otherUserId,
            InviteCode = "ABC12345",
            CreatedAt = DateTime.UtcNow
        };

        var leagueMembers = new List<LeagueMember>
        {
            new()
            {
                Id = Guid.NewGuid(),
                LeagueId = league.Id,
                UserId = otherUserId,
                League = league,
                JoinedAt = DateTime.UtcNow
            }
        };

        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);

        var handler = new GetUserLeaguesQueryHandler(dbMock.Object, mapper);
        var query = new GetUserLeaguesQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Order_By_JoinedAt_Descending()
    {
        // Arrange
        var mapper = CreateMapper();
        var userId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        var league1 = new League
        {
            Id = Guid.NewGuid(),
            Name = "Old League",
            TournamentId = tournamentId,
            OwnerId = userId,
            InviteCode = "ABC12345",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var league2 = new League
        {
            Id = Guid.NewGuid(),
            Name = "Recent League",
            TournamentId = tournamentId,
            OwnerId = userId,
            InviteCode = "DEF67890",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var league3 = new League
        {
            Id = Guid.NewGuid(),
            Name = "Middle League",
            TournamentId = tournamentId,
            OwnerId = userId,
            InviteCode = "GHI11111",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var leagueMembers = new List<LeagueMember>
        {
            new()
            {
                Id = Guid.NewGuid(),
                LeagueId = league1.Id,
                UserId = userId,
                League = league1,
                JoinedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                Id = Guid.NewGuid(),
                LeagueId = league2.Id,
                UserId = userId,
                League = league2,
                JoinedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                LeagueId = league3.Id,
                UserId = userId,
                League = league3,
                JoinedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);

        var handler = new GetUserLeaguesQueryHandler(dbMock.Object, mapper);
        var query = new GetUserLeaguesQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(3);
        result.Data[0].Name.Should().Be("Recent League");
        result.Data[1].Name.Should().Be("Middle League");
        result.Data[2].Name.Should().Be("Old League");
    }

    [Fact]
    public async Task Handle_Should_Return_Success_For_NonExistent_User()
    {
        // Arrange
        var mapper = CreateMapper();
        var nonExistentUserId = Guid.NewGuid();

        var leagueMembers = new List<LeagueMember>();
        var leagueMembersDbSetMock = leagueMembers.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.LeagueMembers).Returns(leagueMembersDbSetMock.Object);

        var handler = new GetUserLeaguesQueryHandler(dbMock.Object, mapper);
        var query = new GetUserLeaguesQuery(nonExistentUserId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
