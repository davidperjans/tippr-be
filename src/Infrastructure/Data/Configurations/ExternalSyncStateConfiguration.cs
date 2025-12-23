using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ExternalSyncStateConfiguration : IEntityTypeConfiguration<ExternalSyncState>
    {
        public void Configure(EntityTypeBuilder<ExternalSyncState> builder)
        {
            builder.ToTable("ExternalSyncStates");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.TournamentId)
                .IsRequired();

            builder.Property(e => e.Provider)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("ApiFootball");

            builder.Property(e => e.Resource)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.LastSyncedAt)
                .IsRequired();

            builder.Property(e => e.NextAllowedSyncAt)
                .IsRequired(false);

            builder.Property(e => e.LastHash)
                .HasMaxLength(64);

            builder.Property(e => e.LastError)
                .HasMaxLength(1000);

            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(e => e.UpdatedAt)
                .IsRequired();

            // Composite unique constraint - one sync state per provider/resource per tournament
            builder.HasIndex(e => new { e.TournamentId, e.Provider, e.Resource })
                .IsUnique();

            // Indexes
            builder.HasIndex(e => e.TournamentId);

            // Relationships
            builder.HasOne(e => e.Tournament)
                .WithMany()
                .HasForeignKey(e => e.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
