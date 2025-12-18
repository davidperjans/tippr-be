using API.IntegrationTests.Common;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.IntegrationTests.Teams;

public sealed class TeamsControllerTests : IClassFixture<TipprWebApplicationFactory>
{
    private readonly TipprWebApplicationFactory _factory;

    public TeamsControllerTests(TipprWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTeams_Should_Return_401_When_Not_Authenticated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var res = await client.GetAsync($"/api/teams?tournamentId={Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task GetTeamsByTournament_Should_Return_200_And_Teams_With_Flag_Data()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        var tournamentId = Guid.NewGuid();
        await TestSeed.SeedUserAsync(_factory.Services);
        await TestSeed.SeedTournamentAsync(_factory.Services, tournamentId);

        var team1Id = Guid.NewGuid();
        var team2Id = Guid.NewGuid();

        await SeedTeamsAsync(_factory.Services, tournamentId,
            new TeamSeed(team1Id, "Sweden", "SWE", "https://flags.example/swe.png", "A", 1001),
            new TeamSeed(team2Id, "Spain", "ESP", "https://flags.example/esp.png", "A", 1002)
        );

        // Act
        var res = await client.GetAsync($"/api/teams?tournamentId={tournamentId}");
        var body = await res.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var teams = ExtractArray(body);

        Assert.True(teams.Count >= 2);

        // Verifiera att “flag/data” verkligen finns med (acceptance)
        var sweden = teams.FirstOrDefault(x => GetString(x, "name") == "Sweden");
        Assert.NotNull(sweden);

        Assert.Equal(team1Id, GetGuid(sweden!, "id"));
        Assert.Equal(tournamentId, GetGuid(sweden!, "tournamentId"));

        // "flagUrl" (eller motsvarande) ska finnas
        Assert.Equal("https://flags.example/swe.png", GetString(sweden!, "flagUrl"));

        // Extra data som ofta finns på team
        Assert.Equal("SWE", GetString(sweden!, "code"));
        Assert.Equal("A", GetString(sweden!, "groupName"));
        Assert.Equal(1001, GetInt(sweden!, "apiFootballId"));
    }

    [Fact]
    public async Task GetTeam_Should_Return_200_And_Team()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        var tournamentId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        await TestSeed.SeedUserAsync(_factory.Services);
        await TestSeed.SeedTournamentAsync(_factory.Services, tournamentId);

        await SeedTeamsAsync(_factory.Services, tournamentId,
            new TeamSeed(teamId, "France", "FRA", "https://flags.example/fra.png", "B", 2001)
        );

        // Act
        var res = await client.GetAsync($"/api/teams/{teamId}");
        var body = await res.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var team = ExtractObject(body);

        Assert.Equal(teamId, GetGuid(team, "id"));
        Assert.Equal(tournamentId, GetGuid(team, "tournamentId"));
        Assert.Equal("France", GetString(team, "name"));
        Assert.Equal("FRA", GetString(team, "code"));
        Assert.Equal("https://flags.example/fra.png", GetString(team, "flagUrl"));
        Assert.Equal("B", GetString(team, "groupName"));
        Assert.Equal(2001, GetInt(team, "apiFootballId"));
    }

    [Fact]
    public async Task GetTeam_Should_Return_404_When_Not_Found()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();
        await TestSeed.SeedUserAsync(_factory.Services);

        // Act
        var res = await client.GetAsync($"/api/teams/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    // ----------------------------
    // Local seeding helper (Team)
    // ----------------------------

    private static async Task SeedTeamsAsync(IServiceProvider services, Guid tournamentId, params TeamSeed[] teams)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TipprDbContext>();

        foreach (var t in teams)
        {
            var exists = await db.Teams.AnyAsync(x => x.Id == t.Id);
            if (exists) continue;

            db.Teams.Add(new Team
            {
                Id = t.Id,
                TournamentId = tournamentId,
                Name = t.Name,
                Code = t.Code,
                FlagUrl = t.FlagUrl,
                GroupName = t.GroupName,
                ApiFootballId = t.ApiFootballId
            });
        }

        await db.SaveChangesAsync();
    }

    private sealed record TeamSeed(
        Guid Id,
        string Name,
        string Code,
        string? FlagUrl,
        string? GroupName,
        int? ApiFootballId
    );

    // --------------------------------------
    // Robust JSON extraction helpers
    // Supports:
    // 1) raw array/object
    // 2) wrapper: { data: ... }
    // --------------------------------------

    private static List<JsonElement> ExtractArray(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
            return root.EnumerateArray().Select(x => x.Clone()).ToList();

        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data))
        {
            if (data.ValueKind == JsonValueKind.Array)
                return data.EnumerateArray().Select(x => x.Clone()).ToList();
        }

        throw new InvalidOperationException($"Expected JSON array (or wrapper with data array). Got: {root.ValueKind}");
    }


    private static JsonElement ExtractObject(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("id", out _))
            return root.Clone();

        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data))
        {
            if (data.ValueKind == JsonValueKind.Object)
                return data.Clone();
        }

        throw new InvalidOperationException($"Expected JSON object (or wrapper with data object). Got: {root.ValueKind}");
    }

    private static string? GetString(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var p) && p.ValueKind != JsonValueKind.Null ? p.GetString() : null;

    private static Guid GetGuid(JsonElement el, string prop)
        => Guid.Parse(el.GetProperty(prop).GetString()!);

    private static int? GetInt(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var p) && p.ValueKind != JsonValueKind.Null ? p.GetInt32() : null;
}
