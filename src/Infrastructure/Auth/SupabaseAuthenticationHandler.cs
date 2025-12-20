using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Infrastructure.Auth
{
    public class SupabaseAuthenticationOptions : AuthenticationSchemeOptions { }

    public class SupabaseAuthenticationHandler : AuthenticationHandler<SupabaseAuthenticationOptions>
    {
        private readonly Supabase.Client _supabase;
        public SupabaseAuthenticationHandler(
            IOptionsMonitor<SupabaseAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            Supabase.Client supabase)
            : base(options, logger, encoder)
        {
            _supabase = supabase;
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string? token = null;

            // 1) Först: Authorization header (REST/vanliga calls)
            if (Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
            {
                var authHeader = authHeaderValues.ToString();

                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = authHeader.Substring("Bearer ".Length).Trim();
                }
                else
                {
                    return AuthenticateResult.Fail("Invalid Authorization header format");
                }
            }

            // 2) Om ingen header-token: prova querystring (SignalR/WebSocket)
            if (string.IsNullOrWhiteSpace(token))
            {
                // SignalR använder ofta "access_token"
                if (Request.Query.TryGetValue("access_token", out var queryTokenValues))
                {
                    token = queryTokenValues.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return AuthenticateResult.Fail("Missing access token");
            }

            try
            {
                // Validera token via Supabase
                var user = await _supabase.Auth.GetUser(token);

                if (user == null)
                    return AuthenticateResult.Fail("Invalid token - user not found");

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),

                    new Claim("sub", user.Id),
                    new Claim("email", user.Email ?? ""),
                    new Claim("role", jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "authenticated"),
                    new Claim("auth_user_id", user.Id)
                };

                foreach (var claim in jwtToken.Claims)
                {
                    if (!claims.Any(c => c.Type == claim.Type))
                        claims.Add(new Claim(claim.Type, claim.Value));
                }

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Token validation failed: {Message}", ex.Message);
                return AuthenticateResult.Fail($"Token validation failed: {ex.Message}");
            }
        }
    }
}
