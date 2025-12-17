using Application.Features.Leagues.Commands.UpdateLeagueSettings;
using Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Features.Leagues.Validators;

public sealed class UpdateLeagueSettingsCommandValidatorTests
{
    private readonly UpdateLeagueSettingsCommandValidator _validator = new();

    private UpdateLeagueSettingsCommand CreateValidCommand() => new(
        LeagueId: Guid.NewGuid(),
        UserId: Guid.NewGuid(),
        PredictionMode: PredictionMode.AllAtOnce,
        DeadlineMinutes: 60,
        PointsCorrectScore: 7,
        PointsCorrectOutcome: 3,
        PointsCorrectGoals: 2,
        PointsRoundOf16Team: 2,
        PointsQuarterFinalTeam: 4,
        PointsSemiFinalTeam: 6,
        PointsFinalTeam: 8,
        PointsTopScorer: 20,
        PointsWinner: 20,
        PointsMostGoalsGroup: 10,
        PointsMostConcededGroup: 10,
        AllowLateEdits: false
    );

    [Fact]
    public void Should_Pass_For_Valid_Command()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_LeagueId_Is_Empty()
    {
        // Arrange
        var command = CreateValidCommand() with { LeagueId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LeagueId);
    }

    [Fact]
    public void Should_Fail_When_UserId_Is_Empty()
    {
        // Arrange
        var command = CreateValidCommand() with { UserId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Should_Fail_When_DeadlineMinutes_Is_Negative(int deadlineMinutes)
    {
        // Arrange
        var command = CreateValidCommand() with { DeadlineMinutes = deadlineMinutes };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DeadlineMinutes);
    }

    [Fact]
    public void Should_Fail_When_DeadlineMinutes_Exceeds_24_Hours()
    {
        // Arrange
        var command = CreateValidCommand() with { DeadlineMinutes = 24 * 60 + 1 }; // 1441 minutes

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DeadlineMinutes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(24 * 60)]
    public void Should_Pass_When_DeadlineMinutes_Is_Valid(int deadlineMinutes)
    {
        // Arrange
        var command = CreateValidCommand() with { DeadlineMinutes = deadlineMinutes };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DeadlineMinutes);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Should_Fail_When_PointsCorrectScore_Is_Out_Of_Range(int points)
    {
        // Arrange
        var command = CreateValidCommand() with { PointsCorrectScore = points };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PointsCorrectScore);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Should_Pass_When_PointsCorrectScore_Is_Valid(int points)
    {
        // Arrange
        var command = CreateValidCommand() with { PointsCorrectScore = points };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PointsCorrectScore);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Should_Fail_When_PointsCorrectOutcome_Is_Out_Of_Range(int points)
    {
        // Arrange
        var command = CreateValidCommand() with { PointsCorrectOutcome = points };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PointsCorrectOutcome);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Should_Fail_When_PointsCorrectGoals_Is_Out_Of_Range(int points)
    {
        // Arrange
        var command = CreateValidCommand() with { PointsCorrectGoals = points };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PointsCorrectGoals);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Should_Fail_When_PointsRoundOf16Team_Is_Out_Of_Range(int points)
    {
        // Arrange
        var command = CreateValidCommand() with { PointsRoundOf16Team = points };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PointsRoundOf16Team);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Should_Fail_When_PointsQuarterFinalTeam_Is_Out_Of_Range(int points)
    {
        // Arrange
        var command = CreateValidCommand() with { PointsQuarterFinalTeam = points };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PointsQuarterFinalTeam);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Should_Fail_When_PointsSemiFinalTeam_Is_Out_Of_Range(int points)
    {
        // Arrange
        var command = CreateValidCommand() with { PointsSemiFinalTeam = points };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PointsSemiFinalTeam);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Should_Fail_When_PointsFinalTeam_Is_Out_Of_Range(int points)
    {
        // Arrange
        var command = CreateValidCommand() with { PointsFinalTeam = points };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PointsFinalTeam);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(201)]
    public void Should_Fail_When_PointsTopScorer_Is_Out_Of_Range(int points)
    {
        // Arrange
        var command = CreateValidCommand() with { PointsTopScorer = points };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PointsTopScorer);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(200)]
    public void Should_Pass_When_PointsTopScorer_Is_Valid(int points)
    {
        // Arrange
        var command = CreateValidCommand() with { PointsTopScorer = points };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PointsTopScorer);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(201)]
    public void Should_Fail_When_PointsWinner_Is_Out_Of_Range(int points)
    {
        // Arrange
        var command = CreateValidCommand() with { PointsWinner = points };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PointsWinner);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(201)]
    public void Should_Fail_When_PointsMostGoalsGroup_Is_Out_Of_Range(int points)
    {
        // Arrange
        var command = CreateValidCommand() with { PointsMostGoalsGroup = points };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PointsMostGoalsGroup);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(201)]
    public void Should_Fail_When_PointsMostConcededGroup_Is_Out_Of_Range(int points)
    {
        // Arrange
        var command = CreateValidCommand() with { PointsMostConcededGroup = points };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PointsMostConcededGroup);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_Pass_For_AllowLateEdits_Value(bool allowLateEdits)
    {
        // Arrange
        var command = CreateValidCommand() with { AllowLateEdits = allowLateEdits };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(PredictionMode.AllAtOnce)]
    [InlineData(PredictionMode.StageByStage)]
    [InlineData(PredictionMode.MatchByMatch)]
    public void Should_Pass_For_All_PredictionModes(PredictionMode mode)
    {
        // Arrange
        var command = CreateValidCommand() with { PredictionMode = mode };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PredictionMode);
    }
}
