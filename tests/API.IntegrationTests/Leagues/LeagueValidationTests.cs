using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.Common;
using FluentAssertions;
using Xunit;

namespace API.IntegrationTests.Leagues;

public sealed class LeagueValidationTests : IClassFixture<TipprWebApplicationFactory>
{
    private readonly TipprWebApplicationFactory _factory;

    // ÄNDRA DEN HÄR OM DIN ROUTE ÄR ANNORLUNDA
    private const string CreateLeagueRoute = "/api/leagues";
    // Exempel om du har PascalCase: "/api/v1/Leagues"

    public LeagueValidationTests(TipprWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateLeague_Should_Return_400_With_Clear_Validation_Errors_When_Request_Is_Invalid()
    {
        // Arrange
        await TestSeed.SeedUserAsync(_factory.Services); // behövs för TestAuthHandler -> user_id claim

        var client = _factory.CreateAuthenticatedClient();

        var payload = new
        {
            name = "",                  // invalid
            description = (string?)null,
            tournamentId = Guid.Empty,  // invalid
            isPublic = true,
            maxMembers = 10,
            imageUrl = (string?)null
        };

        // Act
        var response = await client.PostAsJsonAsync(CreateLeagueRoute, payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Name");
        body.Should().Contain("TournamentId");
    }
}
