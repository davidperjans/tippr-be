using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.Mapping;
using Application.Features.Leagues.Queries.GetLeague;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Leagues.Queries;

public sealed class GetLeagueQueryHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<LeagueProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_Return_League_When_Found()
    {
        // Arrange
        var mapper = CreateMapper();
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        var leagues = new List<League>
        {
            new()
            {
                Id = leagueId,
                Name = "Test League",
                Description = "A test league for testing",
                TournamentId = tournamentId,
                OwnerId = ownerId,
                InviteCode = "ABC12345",
                IsPublic = true,
                IsGlobal = false,
                MaxMembers = 50,
                ImageUrl = "https://example.com/league.png",
                CreatedAt = DateTime.UtcNow
            }
        };

        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new GetLeagueQueryHandler(dbMock.Object, mapper);
        var query = new GetLeagueQuery(leagueId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Test League");
        result.Data.Description.Should().Be("A test league for testing");
        result.Data.TournamentId.Should().Be(tournamentId);
        result.Data.OwnerId.Should().Be(ownerId);
        result.Data.InviteCode.Should().Be("ABC12345");
        result.Data.IsPublic.Should().BeTrue();
        result.Data.IsGlobal.Should().BeFalse();
        result.Data.MaxMembers.Should().Be(50);
        result.Data.ImageUrl.Should().Be("https://example.com/league.png");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_League_Not_Found()
    {
        // Arrange
        var mapper = CreateMapper();
        var nonExistentId = Guid.NewGuid();

        var leagues = new List<League>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Other League",
                TournamentId = Guid.NewGuid(),
                OwnerId = Guid.NewGuid(),
                InviteCode = "XYZ98765",
                CreatedAt = DateTime.UtcNow
            }
        };

        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new GetLeagueQueryHandler(dbMock.Object, mapper);
        var query = new GetLeagueQuery(nonExistentId);

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
        var mapper = CreateMapper();
        var anyId = Guid.NewGuid();

        var leagues = new List<League>();
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new GetLeagueQueryHandler(dbMock.Object, mapper);
        var query = new GetLeagueQuery(anyId);

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
        var mapper = CreateMapper();
        var leagueId = Guid.NewGuid();

        var leagues = new List<League>
        {
            new()
            {
                Id = leagueId,
                Name = "Minimal League",
                Description = null,
                TournamentId = Guid.NewGuid(),
                OwnerId = Guid.NewGuid(),
                InviteCode = "MIN00001",
                IsPublic = false,
                MaxMembers = null,
                ImageUrl = null,
                CreatedAt = DateTime.UtcNow
            }
        };

        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new GetLeagueQueryHandler(dbMock.Object, mapper);
        var query = new GetLeagueQuery(leagueId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Description.Should().BeNull();
        result.Data.MaxMembers.Should().BeNull();
        result.Data.ImageUrl.Should().BeNull();
    }
}
