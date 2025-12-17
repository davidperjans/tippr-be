using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.Common;
using Domain.Enums;
using FluentAssertions;

namespace API.IntegrationTests.Tournaments;

public class TournamentsControllerTests : IntegrationTestBase
{
    public TournamentsControllerTests(TipprWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetTournaments_Should_Return_All_Tournaments()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        await CreateTestTournamentAsync("World Cup 2026", 2026);
        await CreateTestTournamentAsync("Euro 2028", 2028);

        Client.WithTestUser(user.Id, user.AuthUserId, user.Email);

        // Act
        var response = await Client.GetAsync("/api/tournaments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TournamentListResponse>();
        result.Should().NotBeNull();
        result!.Data.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetTournamentById_Should_Return_Tournament_When_Exists()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var tournament = await CreateTestTournamentAsync("World Cup 2026", 2026);

        Client.WithTestUser(user.Id, user.AuthUserId, user.Email);

        // Act
        var response = await Client.GetAsync($"/api/tournaments/{tournament.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TournamentResponse>();
        result.Should().NotBeNull();
        result!.Data.Name.Should().Be("World Cup 2026");
        result.Data.Year.Should().Be(2026);
    }

    [Fact]
    public async Task GetTournamentById_Should_Return_NotFound_When_Not_Exists()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        Client.WithTestUser(user.Id, user.AuthUserId, user.Email);

        // Act
        var response = await Client.GetAsync($"/api/tournaments/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTournament_Should_Return_Unauthorized_For_Non_Admin()
    {
        // Arrange
        var user = await CreateTestUserAsync(role: UserRole.User);
        Client.WithTestUser(user.Id, user.AuthUserId, user.Email);

        var request = new
        {
            Name = "Test Tournament",
            Year = 2027,
            StartDate = new DateTime(2027, 6, 1),
            EndDate = new DateTime(2027, 7, 31)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/tournaments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTournament_Should_Succeed_For_Admin()
    {
        // Arrange
        var admin = await CreateTestUserAsync(
            email: "admin@test.com",
            username: "admin",
            role: UserRole.Admin);

        Client.WithTestAdmin(admin.Id, admin.AuthUserId, admin.Email);

        var request = new
        {
            Name = "New Tournament",
            Year = 2030,
            StartDate = new DateTime(2030, 6, 1),
            EndDate = new DateTime(2030, 7, 31)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/tournaments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // Response DTOs for deserialization
    private record TournamentListResponse(bool IsSuccess, List<TournamentDto> Data);
    private record TournamentResponse(bool IsSuccess, TournamentDto Data);
    private record TournamentDto(Guid Id, string Name, int Year, DateTime StartDate, DateTime EndDate, bool IsActive);
}
