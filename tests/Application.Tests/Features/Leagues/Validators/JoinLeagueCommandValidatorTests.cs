using Application.Features.Leagues.Commands.JoinLeague;
using FluentValidation.TestHelper;

namespace Application.Tests.Features.Leagues.Validators;

public sealed class JoinLeagueCommandValidatorTests
{
    private readonly JoinLeagueCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_For_Valid_Command()
    {
        // Arrange
        var command = new JoinLeagueCommand(
            LeagueId: Guid.NewGuid(),
            InviteCode: "ABC12345"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_For_Valid_Command_With_Null_InviteCode()
    {
        // Arrange
        var command = new JoinLeagueCommand(
            LeagueId: Guid.NewGuid(),
            InviteCode: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_LeagueId_Is_Empty()
    {
        // Arrange
        var command = new JoinLeagueCommand(
            LeagueId: Guid.Empty,
            InviteCode: "ABC12345"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LeagueId);
    }

    [Fact]
    public void Should_Fail_When_InviteCode_Exceeds_20_Characters()
    {
        // Arrange
        var longCode = new string('A', 21);
        var command = new JoinLeagueCommand(
            LeagueId: Guid.NewGuid(),
            InviteCode: longCode
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InviteCode);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("ABC")]
    [InlineData("ABCDEFGH")]
    [InlineData("12345678901234567890")]
    public void Should_Pass_For_Valid_InviteCode_Lengths(string inviteCode)
    {
        // Arrange
        var command = new JoinLeagueCommand(
            LeagueId: Guid.NewGuid(),
            InviteCode: inviteCode
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.InviteCode);
    }
}
