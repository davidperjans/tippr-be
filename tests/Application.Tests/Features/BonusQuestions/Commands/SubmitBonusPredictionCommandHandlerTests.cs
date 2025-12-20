using Application.Common;
using Application.Common.Interfaces;
using Application.Features.BonusQuestions.Commands.SubmitBonusPrediction;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.Tests.Features.BonusQuestions.Commands;

public sealed class SubmitBonusPredictionCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_BonusPrediction_When_Valid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var bonusQuestionId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        var tournament = new Tournament
        {
            Id = tournamentId,
            Name = "Euro 2024",
            Year = 2024,
            Type = TournamentType.EuroCup,
            StartDate = DateTime.UtcNow.AddMonths(1), // not started yet
            EndDate = DateTime.UtcNow.AddMonths(2),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = userId,
            TournamentId = tournamentId,
            Tournament = tournament,
            InviteCode = "ABC12345",
            IsPublic = true,
            CreatedAt = DateTime.UtcNow
        };

        var leagueMember = new LeagueMember
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        var bonusQuestion = new BonusQuestion
        {
            Id = bonusQuestionId,
            TournamentId = tournamentId,
            QuestionType = BonusQuestionType.Winner,
            Question = "Who will win?",
            Points = 10,
            IsResolved = false,
            CreatedAt = DateTime.UtcNow
        };

        var team = new Team
        {
            Id = teamId,
            Name = "France",
            TournamentId = tournamentId,
            CreatedAt = DateTime.UtcNow
        };

        var bonusPredictions = new List<BonusPrediction>();
        var bonusPredictionsDbSetMock = bonusPredictions.BuildMockDbSet();

        bonusPredictionsDbSetMock
            .Setup(x => x.Add(It.IsAny<BonusPrediction>()))
            .Callback<BonusPrediction>(bp => bonusPredictions.Add(bp));

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(new List<League> { league }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(new List<LeagueMember> { leagueMember }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.BonusQuestions).Returns(new List<BonusQuestion> { bonusQuestion }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.BonusPredictions).Returns(bonusPredictionsDbSetMock.Object);
        dbMock.Setup(x => x.Teams).Returns(new List<Team> { team }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new SubmitBonusPredictionCommandHandler(dbMock.Object, currentUserMock.Object);

        var cmd = new SubmitBonusPredictionCommand(
            LeagueId: leagueId,
            BonusQuestionId: bonusQuestionId,
            AnswerTeamId: teamId,
            AnswerText: null
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);

        bonusPredictions.Should().HaveCount(1);
        bonusPredictions[0].UserId.Should().Be(userId);
        bonusPredictions[0].LeagueId.Should().Be(leagueId);
        bonusPredictions[0].BonusQuestionId.Should().Be(bonusQuestionId);
        bonusPredictions[0].AnswerTeamId.Should().Be(teamId);
        bonusPredictions[0].PointsEarned.Should().BeNull();

        dbMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_League_Not_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(new List<League>().BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new SubmitBonusPredictionCommandHandler(dbMock.Object, currentUserMock.Object);

        var cmd = new SubmitBonusPredictionCommand(leagueId, Guid.NewGuid(), Guid.NewGuid(), null);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error!.Code.Should().Be("league.not_found");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Not_League_Member()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        var tournament = new Tournament
        {
            Id = tournamentId,
            StartDate = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        };

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = otherUserId,
            TournamentId = tournamentId,
            Tournament = tournament,
            InviteCode = "ABC12345",
            CreatedAt = DateTime.UtcNow
        };

        var leagueMember = new LeagueMember
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = otherUserId, // different user is member
            JoinedAt = DateTime.UtcNow
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(new List<League> { league }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(new List<LeagueMember> { leagueMember }.BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new SubmitBonusPredictionCommandHandler(dbMock.Object, currentUserMock.Object);

        var cmd = new SubmitBonusPredictionCommand(leagueId, Guid.NewGuid(), Guid.NewGuid(), null);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        result.Error!.Code.Should().Be("league.not_member");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Tournament_Already_Started()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var bonusQuestionId = Guid.NewGuid();

        var tournament = new Tournament
        {
            Id = tournamentId,
            StartDate = DateTime.UtcNow.AddDays(-1), // already started
            CreatedAt = DateTime.UtcNow
        };

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = userId,
            TournamentId = tournamentId,
            Tournament = tournament,
            InviteCode = "ABC12345",
            CreatedAt = DateTime.UtcNow
        };

        var leagueMember = new LeagueMember
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        var bonusQuestion = new BonusQuestion
        {
            Id = bonusQuestionId,
            TournamentId = tournamentId,
            QuestionType = BonusQuestionType.Winner,
            Question = "Who will win?",
            Points = 10,
            IsResolved = false,
            CreatedAt = DateTime.UtcNow
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(new List<League> { league }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(new List<LeagueMember> { leagueMember }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.BonusQuestions).Returns(new List<BonusQuestion> { bonusQuestion }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.BonusPredictions).Returns(new List<BonusPrediction>().BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new SubmitBonusPredictionCommandHandler(dbMock.Object, currentUserMock.Object);

        var cmd = new SubmitBonusPredictionCommand(leagueId, bonusQuestionId, Guid.NewGuid(), null);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.BusinessRule);
        result.Error!.Code.Should().Be("bonus_prediction.deadline_passed");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Prediction_Already_Exists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var bonusQuestionId = Guid.NewGuid();

        var tournament = new Tournament
        {
            Id = tournamentId,
            StartDate = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        };

        var league = new League
        {
            Id = leagueId,
            Name = "Test League",
            OwnerId = userId,
            TournamentId = tournamentId,
            Tournament = tournament,
            InviteCode = "ABC12345",
            CreatedAt = DateTime.UtcNow
        };

        var leagueMember = new LeagueMember
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        var bonusQuestion = new BonusQuestion
        {
            Id = bonusQuestionId,
            TournamentId = tournamentId,
            QuestionType = BonusQuestionType.Winner,
            Question = "Who will win?",
            Points = 10,
            IsResolved = false,
            CreatedAt = DateTime.UtcNow
        };

        var existingPrediction = new BonusPrediction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LeagueId = leagueId,
            BonusQuestionId = bonusQuestionId,
            AnswerTeamId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var dbMock = new Mock<ITipprDbContext>();
        dbMock.Setup(x => x.Leagues).Returns(new List<League> { league }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.LeagueMembers).Returns(new List<LeagueMember> { leagueMember }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.BonusQuestions).Returns(new List<BonusQuestion> { bonusQuestion }.BuildMockDbSet().Object);
        dbMock.Setup(x => x.BonusPredictions).Returns(new List<BonusPrediction> { existingPrediction }.BuildMockDbSet().Object);

        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.SetupGet(x => x.UserId).Returns(userId);

        var handler = new SubmitBonusPredictionCommandHandler(dbMock.Object, currentUserMock.Object);

        var cmd = new SubmitBonusPredictionCommand(leagueId, bonusQuestionId, Guid.NewGuid(), null);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error!.Code.Should().Be("bonus_prediction.already_exists");
    }
}
