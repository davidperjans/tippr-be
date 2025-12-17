using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace API.IntegrationTests.Common;

public class TestAuthenticationOptions : AuthenticationSchemeOptions
{
}

public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationOptions>
{
    public const string TestScheme = "TestScheme";
    public const string TestUserIdHeader = "X-Test-User-Id";
    public const string TestAuthUserIdHeader = "X-Test-Auth-User-Id";
    public const string TestEmailHeader = "X-Test-Email";
    public const string TestRoleHeader = "X-Test-Role";

    public TestAuthenticationHandler(
        IOptionsMonitor<TestAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check for anonymous endpoint
        var endpoint = Context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Check for test headers
        if (!Request.Headers.TryGetValue(TestUserIdHeader, out var userIdValue) ||
            !Request.Headers.TryGetValue(TestAuthUserIdHeader, out var authUserIdValue) ||
            !Request.Headers.TryGetValue(TestEmailHeader, out var emailValue))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new("user_id", userIdValue.ToString()),
            new("sub", authUserIdValue.ToString()),
            new("auth_user_id", authUserIdValue.ToString()),
            new("email", emailValue.ToString()),
            new(ClaimTypes.Email, emailValue.ToString()),
            new(ClaimTypes.NameIdentifier, userIdValue.ToString())
        };

        // Add role if present
        if (Request.Headers.TryGetValue(TestRoleHeader, out var roleValue))
        {
            claims.Add(new Claim("role", roleValue.ToString()));
            claims.Add(new Claim(ClaimTypes.Role, roleValue.ToString()));
        }

        var identity = new ClaimsIdentity(claims, TestScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
