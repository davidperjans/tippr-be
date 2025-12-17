using Application.Features.Tournaments.Commands.CreateTournament;
using Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Application.Tests.Features.Tournaments.Validators;

public sealed class CreateTournamentCommandValidatorTests
{
    private readonly CreateTournamentCommandValidator _validator = new();

    [Fact]
    public void Should_Pass_For_Valid_Command()
    {
        // Arrange
        var command = new CreateTournamentCommand(
            Name: "VM 2026",
            Year: 2026,
            Type: TournamentType.WorldCup,
            StartDate: new DateTime(2026, 6, 11),
            EndDate: new DateTime(2026, 7, 19),
            Country: "USA/Canada/Mexico",
            LogoUrl: null
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
        var command = new CreateTournamentCommand(
            Name: name!,
            Year: 2026,
            Type: TournamentType.WorldCup,
            StartDate: new DateTime(2026, 6, 11),
            EndDate: new DateTime(2026, 7, 19),
            Country: "USA",
            LogoUrl: null
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
        var command = new CreateTournamentCommand(
            Name: longName,
            Year: 2026,
            Type: TournamentType.WorldCup,
            StartDate: new DateTime(2026, 6, 11),
            EndDate: new DateTime(2026, 7, 19),
            Country: "USA",
            LogoUrl: null
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
        var command = new CreateTournamentCommand(
            Name: exactName,
            Year: 2026,
            Type: TournamentType.WorldCup,
            StartDate: new DateTime(2026, 6, 11),
            EndDate: new DateTime(2026, 7, 19),
            Country: "USA",
            LogoUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(1900)]
    [InlineData(1800)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Fail_When_Year_Is_1900_Or_Less(int year)
    {
        // Arrange
        var command = new CreateTournamentCommand(
            Name: "VM",
            Year: year,
            Type: TournamentType.WorldCup,
            StartDate: new DateTime(2026, 6, 11),
            EndDate: new DateTime(2026, 7, 19),
            Country: "USA",
            LogoUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Year);
    }

    [Theory]
    [InlineData(1901)]
    [InlineData(2026)]
    [InlineData(2050)]
    public void Should_Pass_When_Year_Is_Greater_Than_1900(int year)
    {
        // Arrange
        var command = new CreateTournamentCommand(
            Name: "VM",
            Year: year,
            Type: TournamentType.WorldCup,
            StartDate: new DateTime(year, 6, 11),
            EndDate: new DateTime(year, 7, 19),
            Country: "USA",
            LogoUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Year);
    }

    [Fact]
    public void Should_Fail_When_EndDate_Is_Before_StartDate()
    {
        // Arrange
        var command = new CreateTournamentCommand(
            Name: "VM 2026",
            Year: 2026,
            Type: TournamentType.WorldCup,
            StartDate: new DateTime(2026, 7, 19),
            EndDate: new DateTime(2026, 6, 11),
            Country: "USA",
            LogoUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Should_Fail_When_EndDate_Equals_StartDate()
    {
        // Arrange
        var sameDate = new DateTime(2026, 6, 11);
        var command = new CreateTournamentCommand(
            Name: "VM 2026",
            Year: 2026,
            Type: TournamentType.WorldCup,
            StartDate: sameDate,
            EndDate: sameDate,
            Country: "USA",
            LogoUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Should_Pass_When_EndDate_Is_After_StartDate()
    {
        // Arrange
        var command = new CreateTournamentCommand(
            Name: "VM 2026",
            Year: 2026,
            Type: TournamentType.WorldCup,
            StartDate: new DateTime(2026, 6, 11),
            EndDate: new DateTime(2026, 6, 12),
            Country: "USA",
            LogoUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    [Theory]
    [InlineData(TournamentType.WorldCup)]
    [InlineData(TournamentType.EuroCup)]
    [InlineData(TournamentType.ChampionsLeague)]
    [InlineData(TournamentType.EuropaLeague)]
    public void Should_Pass_For_All_Tournament_Types(TournamentType type)
    {
        // Arrange
        var command = new CreateTournamentCommand(
            Name: "Tournament",
            Year: 2026,
            Type: type,
            StartDate: new DateTime(2026, 6, 11),
            EndDate: new DateTime(2026, 7, 19),
            Country: "Country",
            LogoUrl: null
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }
}
