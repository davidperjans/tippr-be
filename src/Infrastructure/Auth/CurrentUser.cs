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
        public Guid UserId => Guid.Parse(GetClaimValue("user_id"));

        /// <summary>
        /// Email från Supabase token
        /// </summary>
        public string Email => GetClaimValue("email");

        /// <summary>
        /// Supabase Auth User ID
        /// </summary>
        public Guid AuthUserId => Guid.Parse(GetClaimValue("sub"));

        private string GetClaimValue(string type)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || user.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedException("User is not authenticated.");
            }

            // Försök hitta claim med olika varianter
            var claim = user.FindFirst(type)
                ?? user.FindFirst(type.ToLower())
                ?? user.FindFirst(GetStandardClaimType(type));

            if (claim == null || string.IsNullOrEmpty(claim.Value))
            {
                throw new UnauthorizedException($"Missing '{type}' claim.");
            }

            return claim.Value;
        }

        private string GetStandardClaimType(string type)
        {
            return type.ToLower() switch
            {
                "email" => ClaimTypes.Email,
                "user_id" => ClaimTypes.NameIdentifier,
                "sub" => ClaimTypes.NameIdentifier,
                "auth_user_id" => ClaimTypes.NameIdentifier,
                _ => type
            };
        }
    }
}
