using API.IntegrationTests.Common;
using System.Net;

namespace API.IntegrationTests.Auth
{
    public class AuthControllerTests : IClassFixture<TipprWebApplicationFactory>
    {
        private readonly TipprWebApplicationFactory _factory;
        public AuthControllerTests(TipprWebApplicationFactory factory) => _factory = factory;

        [Fact]
        public async Task GetMe_Should_Return_401_When_Not_Authenticated()
        {
            var client = _factory.CreateClient();
            var res = await client.GetAsync("/api/auth/me");
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [Fact]
        public async Task GetMe_Should_Return_200_When_Authenticated()
        {
            var client = _factory.CreateAuthenticatedClient();

            await TestSeed.SeedUserAsync(_factory.Services);

            var res = await client.GetAsync("/api/auth/me");
            var body = await res.Content.ReadAsStringAsync();

            Console.WriteLine($"Status: {(int)res.StatusCode} {res.StatusCode}");
            Console.WriteLine(body);

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }
    }
}
