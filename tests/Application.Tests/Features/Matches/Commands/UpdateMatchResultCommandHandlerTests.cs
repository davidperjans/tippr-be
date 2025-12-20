using Application.Common.Interfaces;
using Application.Features.Matches.Commands.UpdateMatchResult;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Matches.Commands
{
    public sealed class UpdateMatchResultCommandHandlerTests
    {
        private static Mock<IDbContextTransaction> CreateTransactionMock()
        {
            var txMock = new Mock<IDbContextTransaction>();
            txMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            txMock.Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            txMock.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);
            return txMock;
        }

        [Fact]
        public async Task Handle_Should_Update_Match_Result_And_Score_Predictions()
        {
            // Arrange
            var matchId = Guid.NewGuid();
            var tournamentId = Guid.NewGuid();

            var matches = new List<Domain.Entities.Match>
            {
                new()
                {
                    Id = matchId,
                    TournamentId = tournamentId,
                    Status = MatchStatus.Scheduled,
                    HomeScore = null,
                    AwayScore = null,
                    ResultVersion = 0
                }
            };

            var matchesDbSetMock = matches.BuildMockDbSet();
            var txMock = CreateTransactionMock();

            var dbMock = new Mock<ITipprDbContext>();
            dbMock.Setup(x => x.Matches).Returns(matchesDbSetMock.Object);
            dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            dbMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(txMock.Object);

            var standingsServiceMock = new Mock<IStandingsService>();
            standingsServiceMock
                .Setup(x => x.ScorePredictionsForMatchAsync(matchId, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(5);

            var handler = new UpdateMatchResultCommandHandler(dbMock.Object, standingsServiceMock.Object);

            var cmd = new UpdateMatchResultCommand(
                MatchId: matchId,
                HomeScore: 2,
                AwayScore: 1,
                Status: MatchStatus.FullTime
            );

            // Act
            var result = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            matches[0].HomeScore.Should().Be(2);
            matches[0].AwayScore.Should().Be(1);
            matches[0].Status.Should().Be(MatchStatus.FullTime);
            matches[0].ResultVersion.Should().Be(1);
            matches[0].UpdatedAt.Should().NotBe(default);

            standingsServiceMock.Verify(
                x => x.ScorePredictionsForMatchAsync(matchId, 1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Not_Score_When_Match_Not_Finished()
        {
            // Arrange
            var matchId = Guid.NewGuid();

            var matches = new List<Domain.Entities.Match>
            {
                new()
                {
                    Id = matchId,
                    TournamentId = Guid.NewGuid(),
                    Status = MatchStatus.Scheduled,
                    HomeScore = null,
                    AwayScore = null,
                    ResultVersion = 0
                }
            };

            var matchesDbSetMock = matches.BuildMockDbSet();

            var dbMock = new Mock<ITipprDbContext>();
            dbMock.Setup(x => x.Matches).Returns(matchesDbSetMock.Object);
            dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var standingsServiceMock = new Mock<IStandingsService>();

            var handler = new UpdateMatchResultCommandHandler(dbMock.Object, standingsServiceMock.Object);

            // Status is Live, not FullTime
            var cmd = new UpdateMatchResultCommand(
                MatchId: matchId,
                HomeScore: 1,
                AwayScore: 0,
                Status: MatchStatus.Live
            );

            // Act
            var result = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            matches[0].HomeScore.Should().Be(1);
            matches[0].AwayScore.Should().Be(0);
            matches[0].Status.Should().Be(MatchStatus.Live);
            matches[0].ResultVersion.Should().Be(0); // Not incremented for live matches

            standingsServiceMock.Verify(
                x => x.ScorePredictionsForMatchAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Match_Not_Found()
        {
            // Arrange
            var matchId = Guid.NewGuid();

            var matches = new List<Domain.Entities.Match>();
            var matchesDbSetMock = matches.BuildMockDbSet();

            var dbMock = new Mock<ITipprDbContext>();
            dbMock.Setup(x => x.Matches).Returns(matchesDbSetMock.Object);

            var standingsServiceMock = new Mock<IStandingsService>();

            var handler = new UpdateMatchResultCommandHandler(dbMock.Object, standingsServiceMock.Object);

            var cmd = new UpdateMatchResultCommand(
                MatchId: matchId,
                HomeScore: 2,
                AwayScore: 1,
                Status: MatchStatus.FullTime
            );

            // Act
            var result = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be("match.not_found");
        }

        [Fact]
        public async Task Handle_Should_Increment_ResultVersion_When_Rescoring()
        {
            // Arrange
            var matchId = Guid.NewGuid();

            var matches = new List<Domain.Entities.Match>
            {
                new()
                {
                    Id = matchId,
                    TournamentId = Guid.NewGuid(),
                    Status = MatchStatus.FullTime,
                    HomeScore = 1,
                    AwayScore = 1,
                    ResultVersion = 1
                }
            };

            var matchesDbSetMock = matches.BuildMockDbSet();
            var txMock = CreateTransactionMock();

            var dbMock = new Mock<ITipprDbContext>();
            dbMock.Setup(x => x.Matches).Returns(matchesDbSetMock.Object);
            dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            dbMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(txMock.Object);

            var standingsServiceMock = new Mock<IStandingsService>();
            standingsServiceMock
                .Setup(x => x.ScorePredictionsForMatchAsync(matchId, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(5);

            var handler = new UpdateMatchResultCommandHandler(dbMock.Object, standingsServiceMock.Object);

            // Updating an already finished match (score correction)
            var cmd = new UpdateMatchResultCommand(
                MatchId: matchId,
                HomeScore: 2,
                AwayScore: 1,
                Status: MatchStatus.FullTime
            );

            // Act
            var result = await handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            matches[0].HomeScore.Should().Be(2);
            matches[0].AwayScore.Should().Be(1);
            matches[0].ResultVersion.Should().Be(2);

            standingsServiceMock.Verify(
                x => x.ScorePredictionsForMatchAsync(matchId, 2, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
