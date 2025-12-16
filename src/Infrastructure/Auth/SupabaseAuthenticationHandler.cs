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
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing Authorization header");
            }

            var authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Fail("Invalid Authorization header format");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                // Validera token via Supabase
                var user = await _supabase.Auth.GetUser(token);

                if (user == null)
                {
                    return AuthenticateResult.Fail("Invalid token - user not found");
                }

                // Parsa JWT för att få alla claims
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Skapa claims med både Supabase och .NET standard format
                var claims = new List<Claim>
            {
                // Standard .NET ClaimTypes
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                
                // Supabase standard claims (lowercase)
                new Claim("sub", user.Id),
                new Claim("email", user.Email ?? ""),
                new Claim("role", jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "authenticated"),
                
                // Custom claims för din app
                new Claim("auth_user_id", user.Id)
            };

                // Lägg till alla andra claims från JWT
                foreach (var claim in jwtToken.Claims)
                {
                    if (!claims.Any(c => c.Type == claim.Type))
                    {
                        claims.Add(new Claim(claim.Type, claim.Value));
                    }
                }

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                Logger.LogInformation("Successfully authenticated user: {UserId}, Email: {Email}", user.Id, user.Email);

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
