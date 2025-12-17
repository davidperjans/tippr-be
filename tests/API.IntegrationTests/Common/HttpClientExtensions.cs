namespace API.IntegrationTests.Common;

public static class HttpClientExtensions
{
    public static HttpClient WithTestUser(
        this HttpClient client,
        Guid userId,
        string authUserId,
        string email,
        string? role = null)
    {
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.TestUserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.TestAuthUserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.TestEmailHeader);

        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.TestUserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.TestAuthUserIdHeader, authUserId);
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.TestEmailHeader, email);

        return client;
    }

    public static HttpClient WithTestAdmin(
        this HttpClient client,
        Guid userId,
        string authUserId,
        string email)
    {
        return client.WithTestUser(userId, authUserId, email, "Admin");
    }

    public static HttpClient ClearTestUser(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.TestUserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.TestAuthUserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.TestEmailHeader);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.TestRoleHeader);

        return client;
    }
}
