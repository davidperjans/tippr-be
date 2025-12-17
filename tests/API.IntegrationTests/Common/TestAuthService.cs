using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.IntegrationTests.Common;

public class TestAuthService : IAuthService
{
    private readonly TipprDbContext _dbContext;

    public TestAuthService(TipprDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> GetByAuthUserIdAsync(Guid authUserId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.AuthUserId == authUserId, cancellationToken);
    }

    public async Task<User> GetOrCreateUserAsync(Guid authUserId, string email, CancellationToken cancellationToken = default)
    {
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.AuthUserId == authUserId, cancellationToken);

        if (existingUser != null)
        {
            return existingUser;
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUserId,
            Email = email,
            Username = email.Split('@')[0],
            DisplayName = email.Split('@')[0],
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return newUser;
    }

    public async Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
