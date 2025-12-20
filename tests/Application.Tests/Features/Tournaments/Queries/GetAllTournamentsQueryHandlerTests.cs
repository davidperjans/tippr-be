using Application.Common.Interfaces;
using Application.Features.Tournaments.DTOs;
using Application.Features.Tournaments.Mapping;
using Application.Features.Tournaments.Queries.GetAllTournaments;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Tournaments.Queries;

public sealed class GetAllTournamentsQueryHandlerTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<TournamentProfile>());
        cfg.AssertConfigurationIsValid();
        return cfg.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_Return_All_Tournaments_When_OnlyActive_Is_False()
    {
        // Arrange
        var mapper = CreateMapper();

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
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "VM 2022",
                Year = 2022,
                Type = TournamentType.WorldCup,
                StartDate = new DateTime(2022, 11, 20),
                EndDate = new DateTime(2022, 12, 18),
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var tournamentsDbSetMock = tournaments.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Tournaments).Returns(tournamentsDbSetMock.Object);

        var handler = new GetAllTournamentsQueryHandler(dbMock.Object, mapper);
        var query = new GetAllTournamentsQuery(OnlyActive: false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().Contain(t => t.Name == "VM 2026");
        result.Data.Should().Contain(t => t.Name == "VM 2022");
    }

    [Fact]
    public async Task Handle_Should_Return_Only_Active_Tournaments_When_OnlyActive_Is_True()
    {
        // Arrange
        var mapper = CreateMapper();

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
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "VM 2022",
                Year = 2022,
                Type = TournamentType.WorldCup,
                StartDate = new DateTime(2022, 11, 20),
                EndDate = new DateTime(2022, 12, 18),
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var tournamentsDbSetMock = tournaments.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Tournaments).Returns(tournamentsDbSetMock.Object);

        var handler = new GetAllTournamentsQueryHandler(dbMock.Object, mapper);
        var query = new GetAllTournamentsQuery(OnlyActive: true);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data.Should().Contain(t => t.Name == "VM 2026");
        result.Data.Should().NotContain(t => t.Name == "VM 2022");
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Tournaments_Exist()
    {
        // Arrange
        var mapper = CreateMapper();

        var tournaments = new List<Tournament>();
        var tournamentsDbSetMock = tournaments.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Tournaments).Returns(tournamentsDbSetMock.Object);

        var handler = new GetAllTournamentsQueryHandler(dbMock.Object, mapper);
        var query = new GetAllTournamentsQuery(OnlyActive: false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Order_Tournaments_By_Year_Descending()
    {
        // Arrange
        var mapper = CreateMapper();

        var tournaments = new List<Tournament>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "VM 2018",
                Year = 2018,
                Type = TournamentType.WorldCup,
                StartDate = new DateTime(2018, 6, 14),
                EndDate = new DateTime(2018, 7, 15),
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "VM 2026",
                Year = 2026,
                Type = TournamentType.WorldCup,
                StartDate = new DateTime(2026, 6, 11),
                EndDate = new DateTime(2026, 7, 19),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "VM 2022",
                Year = 2022,
                Type = TournamentType.WorldCup,
                StartDate = new DateTime(2022, 11, 20),
                EndDate = new DateTime(2022, 12, 18),
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        var tournamentsDbSetMock = tournaments.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Tournaments).Returns(tournamentsDbSetMock.Object);

        var handler = new GetAllTournamentsQueryHandler(dbMock.Object, mapper);
        var query = new GetAllTournamentsQuery(OnlyActive: false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(3);
        result.Data[0].Year.Should().Be(2026);
        result.Data[1].Year.Should().Be(2022);
        result.Data[2].Year.Should().Be(2018);
    }
}
