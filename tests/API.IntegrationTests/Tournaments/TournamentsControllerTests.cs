using API.IntegrationTests.Common;
using Application.Features.Tournaments.Commands.CreateTournament;
using Domain.Enums;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace API.IntegrationTests.Tournaments
{
    public class TournamentsControllerTests : IClassFixture<TipprWebApplicationFactory>
    {
        private readonly TipprWebApplicationFactory _factory;
        public TournamentsControllerTests(TipprWebApplicationFactory factory) => _factory = factory;

        [Fact]
        public async Task GetAll_Should_Return_401_When_Not_Authenticated()
        {
            var client = _factory.CreateClient();
            var res = await client.GetAsync("/api/tournaments");
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [Fact]
        public async Task GetById_Should_Return_404_When_Not_Found()
        {
            var client = _factory.CreateAuthenticatedClient();
            var id = Guid.NewGuid();

            var res = await client.GetAsync($"/api/tournaments/{id}");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Return_403_When_Not_Admin()
        {
            await TestSeed.SeedUserAsync(_factory.Services, UserRole.User);

            var client = _factory.CreateAuthenticatedClient();

            var cmd = new CreateTournamentCommand(
                Name: "Test Tournament",
                Year: 2025,
                Type: TournamentType.WorldCup,
                StartDate: new DateTime(2025, 6, 1),
                EndDate: new DateTime(2025, 7, 1),
                Country: "SE",
                LogoUrl: null
            );

            var res = await client.PostAsJsonAsync("/api/tournaments", cmd);
            Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
        }

        [Fact]
        public async Task Create_Should_Succeed_When_Admin()
        {
            // AdminOnly kollar DB: Users.Role == Admin
            await TestSeed.SeedUserAsync(_factory.Services, UserRole.Admin);

            var client = _factory.CreateAuthenticatedClient();

            var cmd = new CreateTournamentCommand(
                Name: "Test Tournament",
                Year: 2025,
                Type: TournamentType.WorldCup,
                StartDate: new DateTime(2025, 6, 1),
                EndDate: new DateTime(2025, 7, 1),
                Country: "SE",
                LogoUrl: null
            );

            var res = await client.PostAsJsonAsync("/api/tournaments", cmd);

            // Om den failar: få ut body direkt (hjälper enormt vid 400/403)
            var body = await res.Content.ReadAsStringAsync();
            Assert.True(res.IsSuccessStatusCode, $"Status: {(int)res.StatusCode} {res.StatusCode}\nBody: {body}");

            // (valfritt) Verifiera Result<Guid> utan att deserialisera din Result-klass
            using var doc = JsonDocument.Parse(body);
            Assert.True(doc.RootElement.GetProperty("isSuccess").GetBoolean());
            var id = doc.RootElement.GetProperty("data").GetGuid();
            Assert.NotEqual(Guid.Empty, id);
        }
    }
}
