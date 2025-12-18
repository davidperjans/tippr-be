using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.IntegrationTests.Common;

public static class TestSeed
{
    public static async Task SeedUserAsync(IServiceProvider services, UserRole role = UserRole.User)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TipprDbContext>();

        var user = await db.Users.FirstOrDefaultAsync(x => x.AuthUserId == TestAuthHandler.AuthUserId);

        if (user == null)
        {
            db.Users.Add(new User
            {
                Id = TestAuthHandler.DefaultInternalUserId, // använd default id vid skapande
                AuthUserId = TestAuthHandler.AuthUserId,
                Username = "testuser",
                DisplayName = "Test User",
                Email = "test@tippr.dev",
                Role = role
            });
        }
        else
        {
            user.Role = role;
            if (string.IsNullOrWhiteSpace(user.Username)) user.Username = "testuser";
            if (string.IsNullOrWhiteSpace(user.DisplayName)) user.DisplayName = "Test User";
            if (string.IsNullOrWhiteSpace(user.Email)) user.Email = "test@tippr.dev";
        }

        await db.SaveChangesAsync();
    }

    public static async Task SeedTournamentAsync(IServiceProvider services, Guid tournamentId)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TipprDbContext>();

        if (!await db.Tournaments.AnyAsync(x => x.Id == tournamentId))
        {
            db.Tournaments.Add(new Tournament
            {
                Id = tournamentId,
                Name = "Test Tournament World Cup",
                Year = 2025,
                Type = TournamentType.WorldCup,
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 12, 31),
                Country = "SE",
                IsActive = true
            });

            await db.SaveChangesAsync();
        }
    }

    public static async Task SeedLeagueAsync(IServiceProvider services, Guid leagueId, Guid tournamentId, string inviteCode = "INV123")
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TipprDbContext>();

        if (!await db.Leagues.AnyAsync(x => x.Id == leagueId))
        {
            db.Leagues.Add(new League
            {
                Id = leagueId,
                Name = "Seed League",
                TournamentId = tournamentId,
                OwnerId = TestAuthHandler.DefaultInternalUserId,
                InviteCode = inviteCode,
                IsPublic = true,
                IsGlobal = false,
                MaxMembers = 10
            });

            await db.SaveChangesAsync();
        }
    }
}
