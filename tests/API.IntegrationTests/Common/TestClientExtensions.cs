namespace API.IntegrationTests.Common;

public static class TestClientExtensions
{
    public static HttpClient CreateAuthenticatedClient(this TipprWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, "1");
        return client;
    }
}
