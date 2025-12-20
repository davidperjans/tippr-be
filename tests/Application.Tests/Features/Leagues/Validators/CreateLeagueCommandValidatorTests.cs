using Application.Features.Leagues.Commands.CreateLeague;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Features.Leagues.Validators;

public sealed class CreateLeagueCommandValidatorTests
{
    private readonly CreateLeagueCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_For_Valid_Command()
    {
        // Arrange
        var command = new CreateLeagueCommand(
            Name: "My League",
            Description: "A test league",
            TournamentId: Guid.NewGuid(),
            IsPublic: true,
            MaxMembers: 50,
            ImageUrl: "https://example.com/image.png"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_For_Minimal_Valid_Command()
    {
        // Arrange
        var command = new CreateLeagueCommand(
            Name: "My League",
            Description: null,
            TournamentId: Guid.NewGuid(),
            IsPublic: false,
            MaxMembers: null,
            ImageUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_When_Name_Is_Empty_Or_Null(string? name)
    {
        // Arrange
        var command = new CreateLeagueCommand(
            Name: name!,
            Description: null,
            TournamentId: Guid.NewGuid(),
            IsPublic: false,
            MaxMembers: null,
            ImageUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_When_Name_Exceeds_100_Characters()
    {
        // Arrange
        var longName = new string('A', 101);
        var command = new CreateLeagueCommand(
            Name: longName,
            Description: null,
            TournamentId: Guid.NewGuid(),
            IsPublic: false,
            MaxMembers: null,
            ImageUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Pass_When_Name_Is_Exactly_100_Characters()
    {
        // Arrange
        var exactName = new string('A', 100);
        var command = new CreateLeagueCommand(
            Name: exactName,
            Description: null,
            TournamentId: Guid.NewGuid(),
            IsPublic: false,
            MaxMembers: null,
            ImageUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_When_TournamentId_Is_Empty()
    {
        // Arrange
        var command = new CreateLeagueCommand(
            Name: "My League",
            Description: null,
            TournamentId: Guid.Empty,
            IsPublic: false,
            MaxMembers: null,
            ImageUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TournamentId);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_500_Characters()
    {
        // Arrange
        var longDescription = new string('A', 501);
        var command = new CreateLeagueCommand(
            Name: "My League",
            Description: longDescription,
            TournamentId: Guid.NewGuid(),
            IsPublic: false,
            MaxMembers: null,
            ImageUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Pass_When_Description_Is_Exactly_500_Characters()
    {
        // Arrange
        var exactDescription = new string('A', 500);
        var command = new CreateLeagueCommand(
            Name: "My League",
            Description: exactDescription,
            TournamentId: Guid.NewGuid(),
            IsPublic: false,
            MaxMembers: null,
            ImageUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Should_Fail_When_MaxMembers_Is_Zero_Or_Negative(int maxMembers)
    {
        // Arrange
        var command = new CreateLeagueCommand(
            Name: "My League",
            Description: null,
            TournamentId: Guid.NewGuid(),
            IsPublic: false,
            MaxMembers: maxMembers,
            ImageUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxMembers);
    }

    [Fact]
    public void Should_Fail_When_MaxMembers_Exceeds_1000()
    {
        // Arrange
        var command = new CreateLeagueCommand(
            Name: "My League",
            Description: null,
            TournamentId: Guid.NewGuid(),
            IsPublic: false,
            MaxMembers: 1001,
            ImageUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxMembers);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Should_Pass_When_MaxMembers_Is_Between_1_And_1000(int maxMembers)
    {
        // Arrange
        var command = new CreateLeagueCommand(
            Name: "My League",
            Description: null,
            TournamentId: Guid.NewGuid(),
            IsPublic: false,
            MaxMembers: maxMembers,
            ImageUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaxMembers);
    }

    [Fact]
    public void Should_Fail_When_ImageUrl_Exceeds_500_Characters()
    {
        // Arrange
        var longUrl = "https://" + new string('a', 495) + ".com";
        var command = new CreateLeagueCommand(
            Name: "My League",
            Description: null,
            TournamentId: Guid.NewGuid(),
            IsPublic: false,
            MaxMembers: null,
            ImageUrl: longUrl
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageUrl);
    }
}
