using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Matches.Mapping;
using Application.Features.Matches.Queries.GetMatch;
using Application.Features.Teams.Mapping;
using AutoMapper;
using Domain.Enums;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Matches.Queries
{
    public sealed class GetMatchQueryHandlerTests
    {
        private static IMapper CreateMapper()
        {
            var cfg = new MapperConfiguration(c =>
            {
                c.AddProfile<MatchProfile>();
                c.AddProfile<TeamProfile>();
            });
            cfg.AssertConfigurationIsValid();
            return cfg.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_Match_When_Found()
        {
            // Arrange
            var mapper = CreateMapper();
            var matchId = Guid.NewGuid();
            var tournamentId = Guid.NewGuid();
            var homeTeamId = Guid.NewGuid();
            var awayTeamId = Guid.NewGuid();

            var matches = new List<Domain.Entities.Match>
            {
                new()
                {
                    Id = matchId,
                    TournamentId = tournamentId,
                    HomeTeamId = homeTeamId,
                    AwayTeamId = awayTeamId,
                    MatchDate = new DateTime(2026, 06, 01, 18, 00, 00, DateTimeKind.Utc),
                    Stage = MatchStage.Group,
                    Status = MatchStatus.Scheduled,
                    HomeScore = null,
                    AwayScore = null,
                    Venue = "Arena",
                    UpdatedAt = DateTime.UtcNow
                }
            };

            var matchesDbSetMock = matches.BuildMockDbSet();

            var dbMock = new Mock<ITipprDbContext>();
            dbMock.Setup(x => x.Matches).Returns(matchesDbSetMock.Object);

            var handler = new GetMatchQueryHandler(dbMock.Object, mapper);
            var query = new GetMatchQuery(matchId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();

            result.Data!.Id.Should().Be(matchId);
            result.Data.TournamentId.Should().Be(tournamentId);
            result.Data.HomeTeamId.Should().Be(homeTeamId);
            result.Data.AwayTeamId.Should().Be(awayTeamId);
            result.Data.Stage.Should().Be(MatchStage.Group);
            result.Data.Status.Should().Be(MatchStatus.Scheduled);
            result.Data.HomeScore.Should().BeNull();
            result.Data.AwayScore.Should().BeNull();
            result.Data.Venue.Should().Be("Arena");
        }

        [Fact]
        public async Task Handle_Should_Return_Failure_When_Match_Not_Found()
        {
            // Arrange
            var mapper = CreateMapper();
            var matches = new List<Domain.Entities.Match>(); // empty
            var matchesDbSetMock = matches.BuildMockDbSet();

            var dbMock = new Mock<ITipprDbContext>();
            dbMock.Setup(x => x.Matches).Returns(matchesDbSetMock.Object);

            var handler = new GetMatchQueryHandler(dbMock.Object, mapper);
            var query = new GetMatchQuery(Guid.NewGuid());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();

            result.Error.Should().NotBeNull();
            result.Error!.Message.Should().Be("match not found");
            result.Error!.Code.Should().Be("match.not_found");
            result.Error!.Type.Should().Be(ErrorType.NotFound);

            result.Data.Should().BeNull();
        }
    }
}
