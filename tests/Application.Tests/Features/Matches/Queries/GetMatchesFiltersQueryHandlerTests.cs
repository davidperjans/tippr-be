using Application.Common.Interfaces;
using Application.Features.Matches.Mapping;
using Application.Features.Matches.Queries.GetMatchesByDate;
using Application.Features.Matches.Queries.GetMatchesByTournament;
using AutoMapper;
using Domain.Enums;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Matches.Queries
{
    public sealed class GetMatchesFiltersQueryHandlerTests
    {
        private static IMapper CreateMapper()
        {
            var cfg = new MapperConfiguration(c =>
            {
                c.AddMaps(typeof(Application.DependencyInjection).Assembly);
            });
            cfg.AssertConfigurationIsValid();
            return cfg.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_Matches_For_Tournament()
        {
            // Arrange
            var mapper = CreateMapper();
            var tournamentId = Guid.NewGuid();

            var home1Id = Guid.NewGuid();
            var away1Id = Guid.NewGuid();

            var matches = new List<Domain.Entities.Match>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    TournamentId = tournamentId,
                    MatchDate = DateTime.UtcNow,
                    Stage = MatchStage.Group,
                    Status = MatchStatus.Scheduled,
                    HomeTeamId = home1Id,
                    AwayTeamId = away1Id,
                    HomeTeam = CreateTeam(home1Id, tournamentId, "Home1"),
                    AwayTeam = CreateTeam(away1Id, tournamentId, "Away1"),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    TournamentId = tournamentId,
                    MatchDate = DateTime.UtcNow.AddHours(2),
                    Stage = MatchStage.Group,
                    Status = MatchStatus.Scheduled,
                    HomeTeamId = Guid.NewGuid(),
                    AwayTeamId = Guid.NewGuid(),
                    HomeTeam = CreateTeam(Guid.NewGuid(), tournamentId, "Home2"),
                    AwayTeam = CreateTeam(Guid.NewGuid(), tournamentId, "Away2"),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    TournamentId = Guid.NewGuid(),
                    MatchDate = DateTime.UtcNow.AddHours(1),
                    Stage = MatchStage.Group,
                    Status = MatchStatus.Scheduled,
                    HomeTeamId = Guid.NewGuid(),
                    AwayTeamId = Guid.NewGuid(),
                    HomeTeam = CreateTeam(Guid.NewGuid(), tournamentId, "Home3"),
                    AwayTeam = CreateTeam(Guid.NewGuid(), tournamentId, "Away3"),
                }
            };

            var matchesDbSetMock = matches.BuildMockDbSet();

            var dbMock = new Mock<ITipprDbContext>();
            dbMock.Setup(x => x.Matches).Returns(matchesDbSetMock.Object);

            var handler = new GetMatchesByTournamentQueryHandler(dbMock.Object);
            var query = new GetMatchesByTournamentQuery(tournamentId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.All(x => x.TournamentId == tournamentId).Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Should_Return_Matches_For_Date()
        {
            // Arrange
            var mapper = CreateMapper();
            var date = new DateOnly(2026, 06, 01);

            var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end = start.AddDays(1);

            var tournamentId = Guid.NewGuid();

            var matches = new List<Domain.Entities.Match>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    TournamentId = tournamentId,
                    MatchDate = start.AddHours(10),
                    Stage = MatchStage.Group,
                    Status = MatchStatus.Scheduled,
                    HomeTeamId = Guid.NewGuid(),
                    AwayTeamId = Guid.NewGuid(),
                    HomeTeam = CreateTeam(Guid.NewGuid(), tournamentId, "HomeA"),
                    AwayTeam = CreateTeam(Guid.NewGuid(), tournamentId, "AwayA"),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    TournamentId = tournamentId,
                    MatchDate = start.AddHours(22),
                    Stage = MatchStage.Group,
                    Status = MatchStatus.Scheduled,
                    HomeTeamId = Guid.NewGuid(),
                    AwayTeamId = Guid.NewGuid(),
                    HomeTeam = CreateTeam(Guid.NewGuid(), tournamentId, "HomeB"),
                    AwayTeam = CreateTeam(Guid.NewGuid(), tournamentId, "AwayB"),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    TournamentId = tournamentId,
                    MatchDate = end.AddMinutes(1),
                    Stage = MatchStage.Group,
                    Status = MatchStatus.Scheduled,
                    HomeTeamId = Guid.NewGuid(),
                    AwayTeamId = Guid.NewGuid(),
                    HomeTeam = CreateTeam(Guid.NewGuid(), tournamentId, "HomeC"),
                    AwayTeam = CreateTeam(Guid.NewGuid(), tournamentId, "AwayC"),
                }
            };

            var matchesDbSetMock = matches.BuildMockDbSet();

            var dbMock = new Mock<ITipprDbContext>();
            dbMock.Setup(x => x.Matches).Returns(matchesDbSetMock.Object);

            var handler = new GetMatchesByDateQueryHandler(dbMock.Object);
            var query = new GetMatchesByDateQuery(date);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);

            // sanity: all returned within [start, end)
            result.Data.All(m => m.MatchDate >= start && m.MatchDate < end).Should().BeTrue();
        }

        private static Domain.Entities.Team CreateTeam(Guid id, Guid tournamentId, string name)
            => new()
            {
                Id = id,
                TournamentId = tournamentId,
                Name = name,
                Code = name[..3].ToUpperInvariant()
            };
    }
}
