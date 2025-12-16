using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Security.Claims;
using Serilog;

namespace Infrastructure.Auth
{
    /// <summary>
    /// Middleware som automatiskt synkar Supabase auth users med intern user databas.
    /// Lägger till user_id claim för enkel access i hela applikationen
    /// </summary>
    public class UserSyncMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDiagnosticContext _diagnosticContext;
        private const string UserCacheKeyPrefix = "user_sync_";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
        public UserSyncMiddleware(RequestDelegate next, IDiagnosticContext diagnosticContext)
        {
            _next = next;
            _diagnosticContext = diagnosticContext;
        }

        public async Task InvokeAsync(
        HttpContext context,
        IAuthService authService,
        IMemoryCache cache)
        {
            // Skip om användaren inte är autentiserad
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var authUserId = ExtractAuthUserId(context.User);
            var email = ExtractEmail(context.User);

            // Om vi inte kan extrahera nödvändig info, fortsätt utan sync
            if (string.IsNullOrEmpty(authUserId) || string.IsNullOrEmpty(email))
            {
                Log.Warning(
                    "Unable to extract auth user info from token. AuthUserId: {AuthUserId}, Email: {Email}",
                    authUserId ?? "null",
                    email ?? "null");

                await _next(context);
                return;
            }

            try
            {
                var user = await GetOrCreateUserWithCacheAsync(
                    authUserId,
                    email,
                    authService,
                    cache);

                if (user != null)
                {
                    // Lägg till user_id claim för enkel access i hela appen
                    EnrichUserClaimsWithUserId(context, user.Id);

                    stopwatch.Stop();

                    // Lägg till i diagnostic context (visas i request log)
                    _diagnosticContext.Set("UserSyncDurationMs", stopwatch.ElapsedMilliseconds);
                    _diagnosticContext.Set("UserId", user.Id);
                    _diagnosticContext.Set("AuthUserId", authUserId);

                    Log.Information(
                        "User sync completed for {Email} (AuthUserId: {AuthUserId}, UserId: {UserId}) in {ElapsedMs}ms",
                        user.Email,
                        authUserId,
                        user.Id,
                        stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    Log.Error(
                        "User sync returned null for AuthUserId: {AuthUserId}",
                        authUserId);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Log men låt requesten fortsätta - vi vill inte bryta hela appen
                Log.Error(
                    ex,
                    "Failed to sync user. AuthUserId: {AuthUserId}, Email: {Email}, Duration: {ElapsedMs}ms",
                    authUserId,
                    email,
                    stopwatch.ElapsedMilliseconds);

                // I production kan vi skicka till monitoring (Sentry, Application Insights, etc)
                // await _errorTracker.CaptureExceptionAsync(ex);
            }

            await _next(context);
        }

        private async Task<Domain.Entities.User?> GetOrCreateUserWithCacheAsync(
            string authUserId,
            string email,
            IAuthService authService,
            IMemoryCache cache)
        {
            var cacheKey = $"{UserCacheKeyPrefix}{authUserId}";

            // Försök hämta från cache först
            if (cache.TryGetValue<Domain.Entities.User>(cacheKey, out var cachedUser))
            {
                Log.Debug(
                    "User retrieved from cache. AuthUserId: {AuthUserId}, UserId: {UserId}",
                    authUserId,
                    cachedUser?.Id);

                return cachedUser;
            }

            // Cache miss - hämta/skapa från databas
            Log.Debug(
                "Cache miss for AuthUserId: {AuthUserId}, fetching from database",
                authUserId);

            var user = await authService.GetOrCreateUserAsync(
                Guid.Parse(authUserId),
                email);

            // Lägg i cache med sliding expiration
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration,
                SlidingExpiration = TimeSpan.FromMinutes(2),
                Priority = CacheItemPriority.Normal
            };

            // Lägg till callback för när cache entry tas bort
            cacheOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                Log.Debug(
                    "User cache entry evicted. Key: {CacheKey}, Reason: {Reason}",
                    key,
                    reason);
            });

            cache.Set(cacheKey, user, cacheOptions);

            return user;
        }

        private void EnrichUserClaimsWithUserId(HttpContext context, Guid userId)
        {
            var existingClaims = context.User.Claims.ToList();

            // Lägg till user_id claim om den inte redan finns
            if (!existingClaims.Any(c => c.Type == "user_id"))
            {
                existingClaims.Add(new Claim("user_id", userId.ToString()));

                var identity = new ClaimsIdentity(
                    existingClaims,
                    context.User.Identity?.AuthenticationType);

                context.User = new ClaimsPrincipal(identity);

                Log.Debug(
                    "Added user_id claim to user context. UserId: {UserId}",
                    userId);
            }
        }

        private string? ExtractAuthUserId(ClaimsPrincipal user)
        {
            return user.FindFirst("sub")?.Value
                ?? user.FindFirst("auth_user_id")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private string? ExtractEmail(ClaimsPrincipal user)
        {
            return user.FindFirst("email")?.Value
                ?? user.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}
