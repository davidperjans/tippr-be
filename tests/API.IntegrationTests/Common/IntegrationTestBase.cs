using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace API.IntegrationTests.Common;

public abstract class IntegrationTestBase : IClassFixture<TipprWebApplicationFactory>, IAsyncLifetime
{
    protected readonly TipprWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(TipprWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual Task DisposeAsync() => Task.CompletedTask;

    protected TipprDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TipprDbContext>();
    }

    protected async Task<User> CreateTestUserAsync(
        string email = "test@test.com",
        string username = "testuser",
        UserRole role = UserRole.User)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TipprDbContext>();

        var user = new User
        {
            Id = Guid.NewGuid(),
            AuthUserId = Guid.NewGuid(),
            Email = email,
            Username = username,
            DisplayName = username,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }

    protected async Task<Tournament> CreateTestTournamentAsync(
        string name = "Test Tournament",
        int year = 2026,
        bool isActive = true)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TipprDbContext>();

        var tournament = new Tournament
        {
            Id = Guid.NewGuid(),
            Name = name,
            Year = year,
            Type = TournamentType.WorldCup,
            Country = "USA",
            StartDate = new DateTime(year, 6, 1),
            EndDate = new DateTime(year, 7, 31),
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };

        db.Tournaments.Add(tournament);
        await db.SaveChangesAsync();

        return tournament;
    }

    protected async Task<League> CreateTestLeagueAsync(
        Guid ownerId,
        Guid tournamentId,
        string name = "Test League",
        bool isPublic = false,
        int? maxMembers = null)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TipprDbContext>();

        var league = new League
        {
            Id = Guid.NewGuid(),
            Name = name,
            OwnerId = ownerId,
            TournamentId = tournamentId,
            IsPublic = isPublic,
            MaxMembers = maxMembers,
            InviteCode = GenerateInviteCode(),
            CreatedAt = DateTime.UtcNow
        };

        db.Leagues.Add(league);

        // Add owner as member
        var membership = new LeagueMember
        {
            Id = Guid.NewGuid(),
            LeagueId = league.Id,
            UserId = ownerId,
            JoinedAt = DateTime.UtcNow
        };

        db.LeagueMembers.Add(membership);

        // Create default settings
        var settings = new LeagueSettings
        {
            Id = Guid.NewGuid(),
            LeagueId = league.Id,
            PredictionMode = PredictionMode.AllAtOnce,
            DeadlineMinutes = 60,
            PointsCorrectScore = 7,
            PointsCorrectOutcome = 3,
            PointsCorrectGoals = 2,
            PointsRoundOf16Team = 2,
            PointsQuarterFinalTeam = 4,
            PointsSemiFinalTeam = 6,
            PointsFinalTeam = 8,
            PointsTopScorer = 20,
            PointsWinner = 20,
            PointsMostGoalsGroup = 10,
            PointsMostConcededGroup = 10,
            AllowLateEdits = false,
            CreatedAt = DateTime.UtcNow
        };

        db.LeagueSettings.Add(settings);

        await db.SaveChangesAsync();

        return league;
    }

    protected async Task AddLeagueMemberAsync(Guid leagueId, Guid userId)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TipprDbContext>();

        var membership = new LeagueMember
        {
            Id = Guid.NewGuid(),
            LeagueId = leagueId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        db.LeagueMembers.Add(membership);
        await db.SaveChangesAsync();
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
