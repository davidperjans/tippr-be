using Application.Common.Interfaces;
using Application.Features.Matches.Commands.UpdateMatchResult;
using Application.Features.Matches.Mapping;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using System.Reflection.Metadata;

namespace Application.Tests.Features.Matches.Commands
{
    public sealed class UpdateMatchResultCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Update_Match_Result()
        {
            // Arrange
            var matchId = Guid.NewGuid();

            var matches = new List<Domain.Entities.Match>
        {
            new()
            {
                Id = matchId,
                Status = MatchStatus.Scheduled,
                HomeScore = null,
                AwayScore = null
            }
        };

            var matchesDbSetMock = matches.BuildMockDbSet();

            var dbMock = new Mock<ITipprDbContext>();
            dbMock.Setup(x => x.Matches).Returns(matchesDbSetMock.Object);
            dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(1);

            var handler = new UpdateMatchResultCommandHandler(dbMock.Object);

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
            matches[0].UpdatedAt.Should().NotBe(default);
            
            dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
