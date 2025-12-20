using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.Commands.LeaveLeague;
using Domain.Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.Leagues.Commands;

public sealed class LeaveLeagueCommandHandlerTests
{
    #region Helper Methods

    private static (LeaveLeagueCommandHandler handler, Mock<ITipprDbContext> dbMock, Mock<Microsoft.EntityFrameworkCore.DbSet<LeagueMember>> membersDbSetMock, Mock<Microsoft.EntityFrameworkCore.DbSet<LeagueStanding>> standingsDbSetMock, Mock<Microsoft.EntityFrameworkCore.DbSet<Prediction>> predictionsDbSetMock, Mock<Microsoft.EntityFrameworkCore.DbSet<BonusPrediction>> bonusPredictionsDbSetMock)
        CreateHandler(
            Guid currentUserId,
            List<League> leagues,
            List<LeagueMember>? members = null,
            List<LeagueStanding>? standings = null,
            List<Prediction>? predictions = null,
            List<BonusPrediction>? bonusPredictions = null)
    {
        members ??= new List<LeagueMember>();
        standings ??= new List<LeagueStanding>();
        predictions ??= new List<Prediction>();
        bonusPredictions ??= new List<BonusPrediction>();

        var leaguesDbSetMock = leagues.BuildMockDbSet();
        var membersDbSetMock = members.BuildMockDbSet();
        var standingsDbSetMock = standings.BuildMockDbSet();
        var predictionsDbSetMock = predictions.BuildMockDbSet();
        var bonusPredictionsDbSetMock = bonusPredictions.BuildMockDbSet();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(leaguesDbSetMock.Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(membersDbSetMock.Object);
        dbMock.Setup(x => x.LeagueStandings).Returns(standingsDbSetMock.Object);
        dbMock.Setup(x => x.Predictions).Returns(predictionsDbSetMock.Object);
        dbMock.Setup(x => x.BonusPredictions).Returns(bonusPredictionsDbSetMock.Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(x => x.UserId).Returns(currentUserId);

        var handler = new LeaveLeagueCommandHandler(dbMock.Object, currentUserMock.Object);

        return (handler, dbMock, membersDbSetMock, standingsDbSetMock, predictionsDbSetMock, bonusPredictionsDbSetMock);
    }

    private static League CreateLeague(
        Guid? id = null,
        Guid? ownerId = null,
        bool isGlobal = false,
        List<LeagueMember>? members = null)
    {
        return new League
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Test League",
            OwnerId = ownerId ?? Guid.NewGuid(),
            TournamentId = Guid.NewGuid(),
            InviteCode = "ABC12345",
            IsGlobal = isGlobal,
            Members = members ?? new List<LeagueMember>(),
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion

    [Fact]
    public async Task Handle_Should_Leave_League_And_Remove_All_User_Data()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var member = new LeagueMember
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = userId,
            IsAdmin = false
        };

        var league = CreateLeague(id: leagueId, ownerId: ownerId, members: new List<LeagueMember> { member });

        var standing = new LeagueStanding
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = userId,
            TotalPoints = 100,
            MatchPoints = 80,
            BonusPoints = 20,
            Rank = 1
        };

        var prediction = new Prediction
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = userId,
            MatchId = Guid.NewGuid(),
            HomeScore = 2,
            AwayScore = 1
        };

        var bonusPrediction = new BonusPrediction
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = userId,
            BonusQuestionId = Guid.NewGuid(),
            AnswerText = "Some answer"
        };

        var leagues = new List<League> { league };
        var members = new List<LeagueMember> { member };
        var standings = new List<LeagueStanding> { standing };
        var predictions = new List<Prediction> { prediction };
        var bonusPredictions = new List<BonusPrediction> { bonusPrediction };

        var (handler, dbMock, membersDbSetMock, standingsDbSetMock, predictionsDbSetMock, bonusPredictionsDbSetMock) =
            CreateHandler(userId, leagues, members, standings, predictions, bonusPredictions);

        var cmd = new LeaveLeagueCommand(leagueId);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        membersDbSetMock.Verify(x => x.Remove(member), Times.Once);
        standingsDbSetMock.Verify(x => x.Remove(standing), Times.Once);
        predictionsDbSetMock.Verify(x => x.RemoveRange(It.Is<IEnumerable<Prediction>>(p => p.Contains(prediction))), Times.Once);
        bonusPredictionsDbSetMock.Verify(x => x.RemoveRange(It.Is<IEnumerable<BonusPrediction>>(bp => bp.Contains(bonusPrediction))), Times.Once);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_League_Not_Found()
    {
        // Arrange
        var nonExistentLeagueId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var (handler, dbMock, _, _, _, _) = CreateHandler(userId, new List<League>());

        var cmd = new LeaveLeagueCommand(nonExistentLeagueId);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("League not found.");
        result.Error.Code.Should().Be("league.not_found");
        result.Error.Type.Should().Be(ErrorType.NotFound);

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_Owner()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var member = new LeagueMember
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = ownerId,
            IsAdmin = true
        };

        var league = CreateLeague(id: leagueId, ownerId: ownerId, members: new List<LeagueMember> { member });
        var leagues = new List<League> { league };

        var (handler, dbMock, _, _, _, _) = CreateHandler(ownerId, leagues);

        var cmd = new LeaveLeagueCommand(leagueId);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("League owner cannot leave. Transfer ownership or delete the league.");
        result.Error.Code.Should().Be("league.owner_cannot_leave");
        result.Error.Type.Should().Be(ErrorType.Forbidden);

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_Not_Member()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var nonMemberUserId = Guid.NewGuid();

        var league = CreateLeague(id: leagueId, ownerId: ownerId);
        var leagues = new List<League> { league };

        var (handler, dbMock, _, _, _, _) = CreateHandler(nonMemberUserId, leagues);

        var cmd = new LeaveLeagueCommand(leagueId);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("You are not a member of this league.");
        result.Error.Code.Should().Be("league.not_member");
        result.Error.Type.Should().Be(ErrorType.NotFound);

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Leaving_Global_League()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var member = new LeagueMember
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = userId,
            IsAdmin = false
        };

        var league = CreateLeague(id: leagueId, ownerId: ownerId, isGlobal: true, members: new List<LeagueMember> { member });
        var leagues = new List<League> { league };

        var (handler, dbMock, _, _, _, _) = CreateHandler(userId, leagues);

        var cmd = new LeaveLeagueCommand(leagueId);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("you cannot leave the global league");
        result.Error.Code.Should().Be("league.cannot_leave_global");
        result.Error.Type.Should().Be(ErrorType.BusinessRule);

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Succeed_When_User_Has_No_Predictions()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var member = new LeagueMember
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = userId,
            IsAdmin = false
        };

        var league = CreateLeague(id: leagueId, ownerId: ownerId, members: new List<LeagueMember> { member });

        var standing = new LeagueStanding
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = userId,
            TotalPoints = 0,
            Rank = 1
        };

        var leagues = new List<League> { league };
        var members = new List<LeagueMember> { member };
        var standings = new List<LeagueStanding> { standing };
        var predictions = new List<Prediction>();
        var bonusPredictions = new List<BonusPrediction>();

        var (handler, dbMock, membersDbSetMock, standingsDbSetMock, _, _) =
            CreateHandler(userId, leagues, members, standings, predictions, bonusPredictions);

        var cmd = new LeaveLeagueCommand(leagueId);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        membersDbSetMock.Verify(x => x.Remove(member), Times.Once);
        standingsDbSetMock.Verify(x => x.Remove(standing), Times.Once);
        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Only_Remove_User_Predictions_For_This_League()
    {
        // Arrange
        var leagueId = Guid.NewGuid();
        var otherLeagueId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var member = new LeagueMember
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = userId,
            IsAdmin = false
        };

        var league = CreateLeague(id: leagueId, ownerId: ownerId, members: new List<LeagueMember> { member });

        var standing = new LeagueStanding
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = userId,
            TotalPoints = 100,
            Rank = 1
        };

        // Prediction in the league user is leaving
        var predictionInLeague = new Prediction
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = userId,
            MatchId = Guid.NewGuid(),
            HomeScore = 2,
            AwayScore = 1
        };

        // Prediction in another league (should NOT be removed)
        var predictionInOtherLeague = new Prediction
        {
            Id = Guid.NewGuid(),
            LeagueId = otherLeagueId,
            UserId = userId,
            MatchId = Guid.NewGuid(),
            HomeScore = 1,
            AwayScore = 0
        };

        var leagues = new List<League> { league };
        var members = new List<LeagueMember> { member };
        var standings = new List<LeagueStanding> { standing };
        var predictions = new List<Prediction> { predictionInLeague, predictionInOtherLeague };
        var bonusPredictions = new List<BonusPrediction>();

        var (handler, _, _, _, predictionsDbSetMock, _) =
            CreateHandler(userId, leagues, members, standings, predictions, bonusPredictions);

        var cmd = new LeaveLeagueCommand(leagueId);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Should only remove prediction from the league being left
        predictionsDbSetMock.Verify(x => x.RemoveRange(It.Is<IEnumerable<Prediction>>(p =>
            p.Count() == 1 &&
            p.Contains(predictionInLeague) &&
            !p.Contains(predictionInOtherLeague)
        )), Times.Once);
    }
}
