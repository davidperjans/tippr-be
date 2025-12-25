using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class MatchLineupSnapshotConfiguration : IEntityTypeConfiguration<MatchLineupSnapshot>
    {
        public void Configure(EntityTypeBuilder<MatchLineupSnapshot> builder)
        {
            builder.ToTable("MatchLineupSnapshots");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.MatchId)
                .IsRequired();

            builder.Property(s => s.Json)
                .IsRequired();

            builder.Property(s => s.FetchedAt)
                .IsRequired();

            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(s => s.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(s => s.MatchId);
            builder.HasIndex(s => s.FetchedAt);

            // Relationships
            builder.HasOne(s => s.Match)
                .WithMany()
                .HasForeignKey(s => s.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class MatchEventsSnapshotConfiguration : IEntityTypeConfiguration<MatchEventsSnapshot>
    {
        public void Configure(EntityTypeBuilder<MatchEventsSnapshot> builder)
        {
            builder.ToTable("MatchEventsSnapshots");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.MatchId)
                .IsRequired();

            builder.Property(s => s.Json)
                .IsRequired();

            builder.Property(s => s.FetchedAt)
                .IsRequired();

            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(s => s.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(s => s.MatchId);
            builder.HasIndex(s => s.FetchedAt);

            // Relationships
            builder.HasOne(s => s.Match)
                .WithMany()
                .HasForeignKey(s => s.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class MatchStatsSnapshotConfiguration : IEntityTypeConfiguration<MatchStatsSnapshot>
    {
        public void Configure(EntityTypeBuilder<MatchStatsSnapshot> builder)
        {
            builder.ToTable("MatchStatsSnapshots");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.MatchId)
                .IsRequired();

            builder.Property(s => s.Json)
                .IsRequired();

            builder.Property(s => s.FetchedAt)
                .IsRequired();

            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(s => s.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(s => s.MatchId);
            builder.HasIndex(s => s.FetchedAt);

            // Relationships
            builder.HasOne(s => s.Match)
                .WithMany()
                .HasForeignKey(s => s.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class MatchHeadToHeadSnapshotConfiguration : IEntityTypeConfiguration<MatchHeadToHeadSnapshot>
    {
        public void Configure(EntityTypeBuilder<MatchHeadToHeadSnapshot> builder)
        {
            builder.ToTable("MatchHeadToHeadSnapshots");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.MatchId)
                .IsRequired();

            builder.Property(s => s.Json)
                .IsRequired();

            builder.Property(s => s.FetchedAt)
                .IsRequired();

            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(s => s.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(s => s.MatchId);
            builder.HasIndex(s => s.FetchedAt);

            // Relationships
            builder.HasOne(s => s.Match)
                .WithMany()
                .HasForeignKey(s => s.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
