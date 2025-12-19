using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.Mapping;
using Application.Features.Leagues.Queries.GetLeague;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Leagues.Queries;

public sealed class GetLeagueQueryHandlerTests
{

    [Fact]
    public async Task Handle_Should_Return_League_When_Found()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var leagues = new List<League>
            {
                CreateLeague(
                    leagueId: leagueId,
                    tournamentId: tournamentId,
                    ownerId: ownerId,
                    name: "Test League",
                    inviteCode: "ABC12345",
                    description: "A test league for testing",
                    isPublic: true,
                    isGlobal: false,
                    maxMembers: 50,
                    imageUrl: "https://example.com/league.png"
                )
            };

        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new GetLeagueQueryHandler(dbMock.Object);
        var query = new GetLeagueQuery(leagueId, ownerId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        result.Data!.Id.Should().Be(leagueId);
        result.Data.Name.Should().Be("Test League");
        result.Data.Description.Should().Be("A test league for testing");
        result.Data.TournamentId.Should().Be(tournamentId);
        result.Data.OwnerId.Should().Be(ownerId);
        result.Data.InviteCode.Should().Be("ABC12345");
        result.Data.IsPublic.Should().BeTrue();
        result.Data.IsGlobal.Should().BeFalse();
        result.Data.MaxMembers.Should().Be(50);
        result.Data.ImageUrl.Should().Be("https://example.com/league.png");

        // Om din LeagueDto innehåller dessa:
        result.Data.IsOwner.Should().BeTrue();
        result.Data.MemberCount.Should().Be(1);
        result.Data.MyRank.Should().Be(1);
        result.Data.MyTotalPoints.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_League_Not_Found()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();

        var leagues = new List<League>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Other League",
                TournamentId = Guid.NewGuid(),
                OwnerId = ownerId,
                InviteCode = "XYZ98765",
                CreatedAt = DateTime.UtcNow
            }
        };

        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new GetLeagueQueryHandler(dbMock.Object);
        var query = new GetLeagueQuery(nonExistentId, ownerId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("league not found");
        result.Error!.Code.Should().Be("league.not_found");
        result.Error!.Type.Should().Be(ErrorType.NotFound);

        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_No_Leagues_Exist()
    {
        // Arrange
        var anyId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var leagues = new List<League>();
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new GetLeagueQueryHandler(dbMock.Object);
        var query = new GetLeagueQuery(anyId, ownerId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("league not found");
        result.Error!.Code.Should().Be("league.not_found");
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_Should_Return_League_With_Null_Optional_Fields()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var leagues = new List<League>
            {
                CreateLeague(
                    leagueId: leagueId,
                    tournamentId: tournamentId,
                    ownerId: ownerId,
                    name: "Minimal League",
                    inviteCode: "MIN00001",
                    description: null,
                    isPublic: false,
                    isGlobal: false,
                    maxMembers: null,
                    imageUrl: null
                )
            };

        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new GetLeagueQueryHandler(dbMock.Object);
        var query = new GetLeagueQuery(leagueId, ownerId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        result.Data!.Id.Should().Be(leagueId);
        result.Data.Name.Should().Be("Minimal League");
        result.Data.Description.Should().BeNull();
        result.Data.MaxMembers.Should().BeNull();
        result.Data.ImageUrl.Should().BeNull();

        // Om din LeagueDto innehåller dessa (som du varit inne på):
        result.Data.IsOwner.Should().BeTrue();
        result.Data.MemberCount.Should().Be(1);
        result.Data.MyRank.Should().Be(1);
        result.Data.MyTotalPoints.Should().Be(0);
    }

    private static League CreateLeague(
            Guid leagueId,
            Guid tournamentId,
            Guid ownerId,
            string name,
            string inviteCode,
            string? description = null,
            bool isPublic = false,
            bool isGlobal = false,
            int? maxMembers = null,
            string? imageUrl = null)
    {
        var league = new League
        {
            Id = leagueId,
            TournamentId = tournamentId,
            OwnerId = ownerId,
            Name = name,
            Description = description,
            InviteCode = inviteCode,
            IsPublic = isPublic,
            IsGlobal = isGlobal,
            MaxMembers = maxMembers,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,

            // Viktigt: Settings får inte vara null (du har gjort den required)
            Settings = new LeagueSettings
            {
                LeagueId = leagueId
                // Sätt fler defaults här om din LeagueSettings kräver det
            },

            // Viktigt: initiera collections så .Count / .Any / .FirstOrDefault inte NRE:ar
            Members = new List<LeagueMember>(),
            Standings = new List<LeagueStanding>()
        };

        // owner är medlem
        league.Members.Add(new LeagueMember
        {
            LeagueId = leagueId,
            UserId = ownerId,
            JoinedAt = DateTime.UtcNow,
            IsAdmin = true,
            IsMuted = false,

            User = new User
            {
                Id = ownerId,
                Username = "owner",
                AvatarUrl = null
            }
        });

        // owner har en standing (så MyRank/MyTotalPoints kan beräknas)
        league.Standings.Add(new LeagueStanding
        {
            LeagueId = leagueId,
            UserId = ownerId,
            Rank = 1,
            TotalPoints = 0,
            MatchPoints = 0,
            BonusPoints = 0
        });

        return league;
    }
}
