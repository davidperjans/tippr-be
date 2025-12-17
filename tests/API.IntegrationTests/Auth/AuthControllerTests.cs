using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.Common;
using FluentAssertions;

namespace API.IntegrationTests.Auth;

public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(TipprWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_User_When_Authenticated()
    {
        // Arrange
        var user = await CreateTestUserAsync(
            email: "testuser@test.com",
            username: "testuser");

        Client.WithTestUser(user.Id, user.AuthUserId, user.Email);

        // Act
        var response = await Client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        result.Should().NotBeNull();
        result!.Data.Email.Should().Be("testuser@test.com");
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        // Arrange - no authentication headers
        Client.ClearTestUser();

        // Act
        var response = await Client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Response DTOs for deserialization
    private record CurrentUserResponse(bool IsSuccess, CurrentUserDto Data);
    private record CurrentUserDto(Guid UserId, string Email, string? DisplayName, DateTime LastLoginAt);
}
