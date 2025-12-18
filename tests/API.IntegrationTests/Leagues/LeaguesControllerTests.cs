using API.Contracts.Leagues;
using API.IntegrationTests.Common;
using Application.Common;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace API.IntegrationTests.Leagues
{
    public class LeaguesControllerTests : IClassFixture<TipprWebApplicationFactory>
    {
        private readonly TipprWebApplicationFactory _factory;

        public LeaguesControllerTests(TipprWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetLeagueById_Should_Return_Failure_When_Not_Found()
        {
            var client = _factory.CreateAuthenticatedClient();
            var id = Guid.NewGuid();
            var res = await client.GetAsync($"/api/leagues/{id}");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }

        [Fact]
        public async Task Create_Then_GetLeague_Should_Return_League()
        {
            var client = _factory.CreateAuthenticatedClient();

            var tournamentId = Guid.NewGuid();
            await TestSeed.SeedUserAsync(_factory.Services);
            await TestSeed.SeedTournamentAsync(_factory.Services, tournamentId);

            var createReq = new CreateLeagueRequest(
                Name: "Test League",
                Description: "Integration test",
                TournamentId: tournamentId,
                IsPublic: true,
                MaxMembers: 10,
                ImageUrl: null
            );

            var createRes = await client.PostAsJsonAsync("/api/leagues", createReq);
            createRes.EnsureSuccessStatusCode();

            var json = await createRes.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            Assert.True(doc.RootElement.GetProperty("isSuccess").GetBoolean());

            var leagueId = doc.RootElement.GetProperty("data").GetGuid();

            var getRes = await client.GetAsync($"/api/leagues/{leagueId}");
            Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);
        }
    }
}
