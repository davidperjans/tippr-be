using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace API.IntegrationTests.Common;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string HeaderName = "X-Test-Auth";

    public static readonly Guid AuthUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DefaultInternalUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly TipprDbContext _db;

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TipprDbContext db)
        : base(options, logger, encoder)
    {
        _db = db;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(HeaderName))
            return AuthenticateResult.Fail("Not authenticated (test)");

        // Hämta intern userId från DB via AuthUserId (unik)
        var internalUserId = await _db.Users
            .AsNoTracking()
            .Where(u => u.AuthUserId == AuthUserId)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        // Om user inte finns än (innan Seed), fallback
        if (internalUserId == Guid.Empty)
            internalUserId = DefaultInternalUserId;

        var claims = new List<Claim>
        {
            new Claim("sub", AuthUserId.ToString()),
            new Claim("email", "test@tippr.dev"),
            new Claim("user_id", internalUserId.ToString()),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}
