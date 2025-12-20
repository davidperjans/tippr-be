using Application.Common.Interfaces;
using Application.Features.Tournaments.Commands.CreateTournament;
using Application.Features.Tournaments.Mapping;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Features.Tournaments.Commands
{
    public sealed class CreateTournamentCommandHandlerTests
    {
        private static IMapper CreateMapper()
        {
            var cfg = new MapperConfiguration(c => c.AddProfile<TournamentProfile>());
            cfg.AssertConfigurationIsValid();
            return cfg.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Create_Tournament_And_Return_Id()
        {
            // Arrange
            var mapper = CreateMapper();

            var tournaments = new List<Tournament>();
            var tournamentsDbSetMock = tournaments.BuildMockDbSet();

            var dbMock = new Mock<ITipprDbContext>();
            dbMock.Setup(x => x.Tournaments).Returns(tournamentsDbSetMock.Object);
            dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var handler = new CreateTournamentCommandHandler(dbMock.Object, mapper);

            var cmd = new CreateTournamentCommand(
                Name: "VM 2026",
                Year: 2026,
                Type: TournamentType.WorldCup,
                StartDate: new DateTime(2026, 6, 11),
                EndDate: new DateTime(2026, 7, 19),
                Country: "USA/Canada/Mexico",
                LogoUrl: null
            );

            // Act
            var result = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBe(Guid.Empty);

            // Verifiera att vi faktiskt sparar
            tournamentsDbSetMock.Verify(s => s.Add(It.IsAny<Tournament>()), Times.Once);
            dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Extra: verifiera att entiteten som addas har defaults satta
            tournamentsDbSetMock.Verify(s => s.Add(It.Is<Tournament>(t =>
                t.Name == "VM 2026" &&
                t.Year == 2026 &&
                t.Type == TournamentType.WorldCup &&
                t.IsActive == true &&
                t.CreatedAt != default &&
                t.Id != Guid.Empty
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Tournament_Already_Exists()
        {
            // Arrange
            var mapper = CreateMapper();

            // Seed: finns redan en Tournament med samma Name + Year (matcha din exists-check!)
            var tournaments = new List<Tournament>
        {
            new Tournament
            {
                Id = Guid.NewGuid(),
                Name = "VM 2026",
                Year = 2026,
                Type = TournamentType.WorldCup,
                StartDate = new DateTime(2026, 6, 11),
                EndDate = new DateTime(2026, 7, 19),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

            var tournamentsDbSetMock = tournaments.BuildMockDbSet();

            var dbMock = new Mock<ITipprDbContext>();
            dbMock.Setup(x => x.Tournaments).Returns(tournamentsDbSetMock.Object);

            var handler = new CreateTournamentCommandHandler(dbMock.Object, mapper);

            var cmd = new CreateTournamentCommand(
                Name: "VM 2026",
                Year: 2026,
                Type: TournamentType.WorldCup,
                StartDate: new DateTime(2026, 6, 11),
                EndDate: new DateTime(2026, 7, 19),
                Country: "USA",
                LogoUrl: null
            );

            // Act
            var result = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();

            // Ska inte skapa/spara något när den redan finns
            tournamentsDbSetMock.Verify(s => s.Add(It.IsAny<Tournament>()), Times.Never);
            dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
