using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class TipprDbContext : DbContext
    {
        public TipprDbContext(DbContextOptions<TipprDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<User> Users { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<LeagueSettings> LeagueSettings { get; set; }
        public DbSet<LeagueMember> LeagueMembers { get; set; }
        public DbSet<Prediction> Predictions { get; set; }
        public DbSet<BonusQuestion> BonusQuestions { get; set; }
        public DbSet<BonusPrediction> BonusPredictions { get; set; }
        public DbSet<LeagueStanding> LeagueStandings { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TipprDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Auto-update timestamps
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Domain.Common.BaseEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (Domain.Common.BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                entity.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
