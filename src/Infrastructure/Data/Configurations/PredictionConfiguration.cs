using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class PredictionConfiguration : IEntityTypeConfiguration<Prediction>
    {
        public void Configure(EntityTypeBuilder<Prediction> builder)
        {
            builder.ToTable("Predictions");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.UserId)
                .IsRequired();

            builder.Property(p => p.MatchId)
                .IsRequired();

            builder.Property(p => p.LeagueId)
                .IsRequired();

            builder.Property(p => p.HomeScore)
                .IsRequired();

            builder.Property(p => p.AwayScore)
                .IsRequired();

            builder.Property(p => p.PointsEarned)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.IsScored)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(p => p.ScoredResultVersion)
                .IsRequired(false);

            builder.Property(p => p.ScoredAt)
                .IsRequired(false);

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(p => p.UpdatedAt)
                .IsRequired();

            // Composite unique constraint - one prediction per user per match per league
            builder.HasIndex(p => new { p.UserId, p.MatchId, p.LeagueId })
                .IsUnique();

            // Indexes
            builder.HasIndex(p => p.UserId);
            builder.HasIndex(p => p.MatchId);
            builder.HasIndex(p => p.LeagueId);
            builder.HasIndex(p => new { p.UserId, p.LeagueId });

            // Relationships
            builder.HasOne(p => p.User)
                .WithMany(u => u.Predictions)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Match)
                .WithMany(m => m.Predictions)
                .HasForeignKey(p => p.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.League)
                .WithMany(l => l.Predictions)
                .HasForeignKey(p => p.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
