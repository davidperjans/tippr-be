using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Seeding
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(TipprDbContext db, CancellationToken ct = default)
        {
            // Kör migration automatiskt om du vill (valfritt i dev)
            // await db.Database.MigrateAsync(ct);

            // 1) Hämta "active" tournament (eller välj hur du vill)
            var tournament = await db.Tournaments
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (tournament == null)
                return;

            // 2) Seeda global league för tournament
            await EnsureGlobalLeagueAsync(db, tournament.Id, ct);
        }

        private static async Task EnsureGlobalLeagueAsync(TipprDbContext db, Guid tournamentId, CancellationToken ct)
        {
            var exists = await db.Leagues.AnyAsync(l => l.TournamentId == tournamentId && l.IsGlobal, ct);
            if (exists) return;

            var leagueId = Guid.NewGuid();

            var league = new League
            {
                Id = leagueId,
                Name = "Global League",
                Description = "Alla tävlar mot alla",
                TournamentId = tournamentId,
                OwnerId = null,                 // ✅ system-owned
                IsPublic = true,                // ✅ alla får gå med
                IsGlobal = true,                // ✅ global flag
                IsSystemCreated = true,         // ✅ system flag
                InviteCode = await GenerateUniqueInviteCodeAsync(db, ct),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Settings = new LeagueSettings
                {
                    Id = Guid.NewGuid(),
                    LeagueId = leagueId,
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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            db.Leagues.Add(league);
            await db.SaveChangesAsync(ct);
        }

        private static async Task<string> GenerateUniqueInviteCodeAsync(TipprDbContext db, CancellationToken ct)
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            const int length = 8;

            while (true)
            {
                var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(length);
                var chars = new char[length];

                for (int i = 0; i < length; i++)
                    chars[i] = alphabet[bytes[i] % alphabet.Length];

                var code = new string(chars);

                var exists = await db.Leagues.AsNoTracking().AnyAsync(l => l.InviteCode == code, ct);
                if (!exists)
                    return code;
            }
        }
    }
}
