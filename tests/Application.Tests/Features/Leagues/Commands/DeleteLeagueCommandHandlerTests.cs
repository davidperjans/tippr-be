using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.Commands.DeleteLeague;
using Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Leagues.Commands;

public sealed class DeleteLeagueCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Delete_League_When_User_Is_Owner()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = ownerId,
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            IsGlobal = false,
            CreatedAt = DateTime.UtcNow
        };

        var leagues = new List<League> { league };
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteLeagueCommandHandler(dbMock.Object);

        var cmd = new DeleteLeagueCommand(leagueId, ownerId);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        leaguesDbSetMock.Verify(x => x.Remove(league), Times.Once);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_League_Not_Found()
    {
        // Arrange
        var nonExistentLeagueId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var leagues = new List<League>();
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new DeleteLeagueCommandHandler(dbMock.Object);

        var cmd = new DeleteLeagueCommand(nonExistentLeagueId, userId);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("League not found.");
        result.Error!.Code.Should().Be("league.not_found");
        result.Error!.Type.Should().Be(ErrorType.NotFound);

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_Not_Owner()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = ownerId,
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            IsGlobal = false,
            CreatedAt = DateTime.UtcNow
        };

        var leagues = new List<League> { league };
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new DeleteLeagueCommandHandler(dbMock.Object);

        var cmd = new DeleteLeagueCommand(leagueId, differentUserId);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("Only the league owner can delete the league.");
        result.Error!.Code.Should().Be("league.forbidden");
        result.Error!.Type.Should().Be(ErrorType.Forbidden);

        leaguesDbSetMock.Verify(x => x.Remove(It.IsAny<League>()), Times.Never);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_League_Is_Global()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var league = new League
        {
            Id = leagueId,
            Name = "Global League",
            OwnerId = ownerId,
            TournamentId = Guid.NewGuid(),
            InviteCode = "GLOBAL123",
            IsGlobal = true,
            CreatedAt = DateTime.UtcNow
        };

        var leagues = new List<League> { league };
        var leaguesDbSetMock = leagues.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);

        var handler = new DeleteLeagueCommandHandler(dbMock.Object);

        var cmd = new DeleteLeagueCommand(leagueId, ownerId);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("Global leagues cannot be deleted.");
        result.Error!.Code.Should().Be("league.global_delete_forbidden");
        result.Error!.Type.Should().Be(ErrorType.Forbidden);

        leaguesDbSetMock.Verify(x => x.Remove(It.IsAny<League>()), Times.Never);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
