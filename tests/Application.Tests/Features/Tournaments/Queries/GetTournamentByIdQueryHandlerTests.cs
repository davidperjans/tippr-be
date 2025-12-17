using Application.Common.Interfaces;
using Application.Features.Tournaments.DTOs;
using Application.Features.Tournaments.Mapping;
using Application.Features.Tournaments.Queries.GetTournamentById;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Tournaments.Queries;

public sealed class GetTournamentByIdQueryHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<TournamentProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_Return_Tournament_When_Found()
    {
        // Arrange
        var mapper = CreateMapper();
        var tournamentId = Guid.NewGuid();

        var tournaments = new List<Tournament>
        {
            new()
            {
                Id = tournamentId,
                Name = "VM 2026",
                Year = 2026,
                Type = TournamentType.WorldCup,
                StartDate = new DateTime(2026, 6, 11),
                EndDate = new DateTime(2026, 7, 19),
                Country = "USA/Canada/Mexico",
                LogoUrl = "https://example.com/logo.png",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var tournamentsDbSetMock = tournaments.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Tournaments).Returns(tournamentsDbSetMock.Object);

        var handler = new GetTournamentByIdQueryHandler(dbMock.Object, mapper);
        var query = new GetTournamentByIdQuery(tournamentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(tournamentId);
        result.Data.Name.Should().Be("VM 2026");
        result.Data.Year.Should().Be(2026);
        result.Data.Type.Should().Be(TournamentType.WorldCup);
        result.Data.Country.Should().Be("USA/Canada/Mexico");
        result.Data.LogoUrl.Should().Be("https://example.com/logo.png");
        result.Data.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Tournament_Not_Found()
    {
        // Arrange
        var mapper = CreateMapper();
        var nonExistentId = Guid.NewGuid();

        var tournaments = new List<Tournament>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "VM 2026",
                Year = 2026,
                Type = TournamentType.WorldCup,
                StartDate = new DateTime(2026, 6, 11),
                EndDate = new DateTime(2026, 7, 19),
                Country = "USA",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        var tournamentsDbSetMock = tournaments.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Tournaments).Returns(tournamentsDbSetMock.Object);

        var handler = new GetTournamentByIdQueryHandler(dbMock.Object, mapper);
        var query = new GetTournamentByIdQuery(nonExistentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("tournament not found");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_No_Tournaments_Exist()
    {
        // Arrange
        var mapper = CreateMapper();
        var anyId = Guid.NewGuid();

        var tournaments = new List<Tournament>();
        var tournamentsDbSetMock = tournaments.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Tournaments).Returns(tournamentsDbSetMock.Object);

        var handler = new GetTournamentByIdQueryHandler(dbMock.Object, mapper);
        var query = new GetTournamentByIdQuery(anyId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("tournament not found");
    }

    [Fact]
    public async Task Handle_Should_Map_All_Properties_Correctly()
    {
        // Arrange
        var mapper = CreateMapper();
        var tournamentId = Guid.NewGuid();
        var startDate = new DateTime(2026, 6, 11, 12, 0, 0);
        var endDate = new DateTime(2026, 7, 19, 22, 0, 0);
        var createdAt = DateTime.UtcNow;

        var tournaments = new List<Tournament>
        {
            new()
            {
                Id = tournamentId,
                Name = "FIFA World Cup 2026",
                Year = 2026,
                Type = TournamentType.WorldCup,
                StartDate = startDate,
                EndDate = endDate,
                Country = "USA/Canada/Mexico",
                LogoUrl = "https://fifa.com/wc2026/logo.png",
                IsActive = true,
                CreatedAt = createdAt
            }
        };

        var tournamentsDbSetMock = tournaments.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Tournaments).Returns(tournamentsDbSetMock.Object);

        var handler = new GetTournamentByIdQueryHandler(dbMock.Object, mapper);
        var query = new GetTournamentByIdQuery(tournamentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        var dto = result.Data!;
        dto.Id.Should().Be(tournamentId);
        dto.Name.Should().Be("FIFA World Cup 2026");
        dto.Year.Should().Be(2026);
        dto.Type.Should().Be(TournamentType.WorldCup);
        dto.StartDate.Should().Be(startDate);
        dto.EndDate.Should().Be(endDate);
        dto.Country.Should().Be("USA/Canada/Mexico");
        dto.LogoUrl.Should().Be("https://fifa.com/wc2026/logo.png");
        dto.IsActive.Should().BeTrue();
    }
}
