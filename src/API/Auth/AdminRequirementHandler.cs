using Application.Common.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Auth
{
    public sealed class AdminRequirementHandler : AuthorizationHandler<AdminRequirement>
    {
        private readonly ITipprDbContext _db;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<AdminRequirementHandler> _logger;

        public AdminRequirementHandler(
            ITipprDbContext db,
            ICurrentUser currentUser,
            ILogger<AdminRequirementHandler> logger)
        {
            _db = db;
            _currentUser = currentUser;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AdminRequirement requirement)
        {
            var userId = _currentUser.UserId;

            var user = await _db.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new { u.Role, u.Username })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning("Admin check failed: User {UserId} not found in database", userId);
                return;
            }

            _logger.LogInformation("Admin check for user {Username} ({UserId}): Role = {Role}, IsAdmin = {IsAdmin}",
                user.Username, userId, user.Role, user.Role == UserRole.Admin);

            if (user.Role == UserRole.Admin)
                context.Succeed(requirement);
        }
    }
}
