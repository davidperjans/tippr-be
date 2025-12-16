using Application.Common.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace API.Auth
{
    public sealed class AdminRequirementHandler : AuthorizationHandler<AdminRequirement>
    {
        private readonly ITipprDbContext _db;
        private readonly ICurrentUser _currentUser;

        public AdminRequirementHandler(ITipprDbContext db, ICurrentUser currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AdminRequirement requirement)
        {
            var userId = _currentUser.UserId;

            var isAdmin = await _db.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.Role == UserRole.Admin)
                .FirstOrDefaultAsync();

            if (isAdmin)
                context.Succeed(requirement);
        }
    }
}
