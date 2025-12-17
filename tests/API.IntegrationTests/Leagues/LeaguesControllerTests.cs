using System.Net;
using System.Net.Http.Json;
using API.IntegrationTests.Common;
using Domain.Enums;
using FluentAssertions;

namespace API.IntegrationTests.Leagues;

public class LeaguesControllerTests : IntegrationTestBase
{
    public LeaguesControllerTests(TipprWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateLeague_Should_Return_Created_When_Valid()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var tournament = await CreateTestTournamentAsync();

        Client.WithTestUser(user.Id, user.AuthUserId, user.Email);

        var request = new
        {
            Name = "My Test League",
            TournamentId = tournament.Id,
            IsPublic = true,
            MaxMembers = 50
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/leagues", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<LeagueCreatedResponse>();
        result.Should().NotBeNull();
        result!.Data.Name.Should().Be("My Test League");
        result.Data.InviteCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateLeague_Should_Return_BadRequest_When_Name_Is_Empty()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var tournament = await CreateTestTournamentAsync();

        Client.WithTestUser(user.Id, user.AuthUserId, user.Email);

        var request = new
        {
            Name = "",
            TournamentId = tournament.Id,
            IsPublic = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/leagues", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserLeagues_Should_Return_User_Leagues()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var tournament = await CreateTestTournamentAsync();
        var league = await CreateTestLeagueAsync(user.Id, tournament.Id, "User's League");

        Client.WithTestUser(user.Id, user.AuthUserId, user.Email);

        // Act
        var response = await Client.GetAsync("/api/leagues");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LeagueListResponse>();
        result.Should().NotBeNull();
        result!.Data.Should().Contain(l => l.Name == "User's League");
    }

    [Fact]
    public async Task GetLeague_Should_Return_League_When_User_Is_Member()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var tournament = await CreateTestTournamentAsync();
        var league = await CreateTestLeagueAsync(user.Id, tournament.Id, "My League");

        Client.WithTestUser(user.Id, user.AuthUserId, user.Email);

        // Act
        var response = await Client.GetAsync($"/api/leagues/{league.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LeagueDetailResponse>();
        result.Should().NotBeNull();
        result!.Data.Name.Should().Be("My League");
    }

    [Fact]
    public async Task GetLeague_Should_Return_Forbidden_When_User_Is_Not_Member()
    {
        // Arrange
        var owner = await CreateTestUserAsync(email: "owner@test.com", username: "owner");
        var otherUser = await CreateTestUserAsync(email: "other@test.com", username: "other");
        var tournament = await CreateTestTournamentAsync();
        var league = await CreateTestLeagueAsync(owner.Id, tournament.Id, "Private League", isPublic: false);

        Client.WithTestUser(otherUser.Id, otherUser.AuthUserId, otherUser.Email);

        // Act
        var response = await Client.GetAsync($"/api/leagues/{league.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task JoinLeague_Should_Succeed_With_Valid_InviteCode()
    {
        // Arrange
        var owner = await CreateTestUserAsync(email: "owner@test.com", username: "owner");
        var joiner = await CreateTestUserAsync(email: "joiner@test.com", username: "joiner");
        var tournament = await CreateTestTournamentAsync();
        var league = await CreateTestLeagueAsync(owner.Id, tournament.Id);

        Client.WithTestUser(joiner.Id, joiner.AuthUserId, joiner.Email);

        var request = new
        {
            InviteCode = league.InviteCode
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/leagues/{league.Id}/join", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task JoinLeague_Should_Fail_With_Invalid_InviteCode()
    {
        // Arrange
        var owner = await CreateTestUserAsync(email: "owner@test.com", username: "owner");
        var joiner = await CreateTestUserAsync(email: "joiner@test.com", username: "joiner");
        var tournament = await CreateTestTournamentAsync();
        var league = await CreateTestLeagueAsync(owner.Id, tournament.Id);

        Client.WithTestUser(joiner.Id, joiner.AuthUserId, joiner.Email);

        var request = new
        {
            InviteCode = "WRONGCODE"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/leagues/{league.Id}/join", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateLeagueSettings_Should_Succeed_For_Owner()
    {
        // Arrange
        var owner = await CreateTestUserAsync();
        var tournament = await CreateTestTournamentAsync();
        var league = await CreateTestLeagueAsync(owner.Id, tournament.Id);

        Client.WithTestUser(owner.Id, owner.AuthUserId, owner.Email);

        var request = new
        {
            PredictionMode = "AllAtOnce",
            DeadlineMinutes = 120,
            PointsCorrectScore = 10,
            PointsCorrectOutcome = 5,
            PointsCorrectGoals = 3,
            PointsRoundOf16Team = 3,
            PointsQuarterFinalTeam = 5,
            PointsSemiFinalTeam = 7,
            PointsFinalTeam = 10,
            PointsTopScorer = 25,
            PointsWinner = 25,
            PointsMostGoalsGroup = 15,
            PointsMostConcededGroup = 15,
            AllowLateEdits = true
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/leagues/{league.Id}/settings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateLeagueSettings_Should_Fail_For_NonOwner()
    {
        // Arrange
        var owner = await CreateTestUserAsync(email: "owner@test.com", username: "owner");
        var member = await CreateTestUserAsync(email: "member@test.com", username: "member");
        var tournament = await CreateTestTournamentAsync();
        var league = await CreateTestLeagueAsync(owner.Id, tournament.Id);

        await AddLeagueMemberAsync(league.Id, member.Id);

        Client.WithTestUser(member.Id, member.AuthUserId, member.Email);

        var request = new
        {
            PredictionMode = "AllAtOnce",
            DeadlineMinutes = 120,
            PointsCorrectScore = 10,
            PointsCorrectOutcome = 5,
            PointsCorrectGoals = 3,
            PointsRoundOf16Team = 3,
            PointsQuarterFinalTeam = 5,
            PointsSemiFinalTeam = 7,
            PointsFinalTeam = 10,
            PointsTopScorer = 25,
            PointsWinner = 25,
            PointsMostGoalsGroup = 15,
            PointsMostConcededGroup = 15,
            AllowLateEdits = true
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/leagues/{league.Id}/settings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetLeagueStandings_Should_Return_Standings_For_Member()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var tournament = await CreateTestTournamentAsync();
        var league = await CreateTestLeagueAsync(user.Id, tournament.Id);

        Client.WithTestUser(user.Id, user.AuthUserId, user.Email);

        // Act
        var response = await Client.GetAsync($"/api/leagues/{league.Id}/standings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // Response DTOs for deserialization
    private record LeagueCreatedResponse(bool IsSuccess, LeagueCreatedDto Data);
    private record LeagueCreatedDto(Guid Id, string Name, string InviteCode);
    private record LeagueListResponse(bool IsSuccess, List<LeagueSummaryDto> Data);
    private record LeagueSummaryDto(Guid Id, string Name, int MemberCount);
    private record LeagueDetailResponse(bool IsSuccess, LeagueDetailDto Data);
    private record LeagueDetailDto(Guid Id, string Name, string InviteCode, Guid OwnerId);
}
