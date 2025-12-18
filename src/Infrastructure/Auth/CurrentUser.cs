using Application.Common.Interfaces;
using Application.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Auth
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Intern User ID (från database)
        /// Läggs till av UserSyncMiddleware
        /// </summary>
        public Guid UserId => Guid.Parse(GetRequiredClaim("user_id"));

        /// <summary>
        /// Email från Supabase token
        /// </summary>
        public string Email => GetRequiredClaim("email");

        /// <summary>
        /// Supabase Auth User ID
        /// </summary>
        public Guid AuthUserId => Guid.Parse(GetRequiredClaim("sub"));

        private string GetRequiredClaim(string type)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedException("User is not authenticated.");

            var claim = user.FindFirst(type);

            if (claim == null || string.IsNullOrWhiteSpace(claim.Value))
                throw new UnauthorizedException($"Missing '{type}' claim.");

            return claim.Value;
        }
    }
}
