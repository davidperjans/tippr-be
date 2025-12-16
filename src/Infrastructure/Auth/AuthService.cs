using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Auth
{
    public class AuthService : IAuthService
    {
        private readonly TipprDbContext _context;
        public AuthService(TipprDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByAuthUserIdAsync(Guid authUserId, CancellationToken cancellationToken = default)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.AuthUserId == authUserId, cancellationToken);
        }

        public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<User> GetOrCreateUserAsync(Guid authUserId, string email, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.AuthUserId == authUserId, cancellationToken);

            if (user == null)
            {
                var username = await GenerateUniqueUsernameAsync(
                    email.Split('@')[0],
                    cancellationToken);

                user = new User
                {
                    Id = Guid.NewGuid(),
                    AuthUserId = authUserId,
                    Email = email,
                    Username = username,
                    DisplayName = email.Split('@')[0],
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = email;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            return user;
        }

        public async Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null) return;

            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<string> GenerateUniqueUsernameAsync(string baseUsername, CancellationToken cancellationToken)
        {
            var username = baseUsername.ToLower();
            var counter = 1;

            while (await _context.Users.AnyAsync(u => u.Username == username, cancellationToken))
            {
                username = $"{baseUsername.ToLower()}{counter}";
                counter++;
            }

            return username;
        }
    }
}
