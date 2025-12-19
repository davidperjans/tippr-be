using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class LeagueConfiguration : IEntityTypeConfiguration<League>
    {
        public void Configure(EntityTypeBuilder<League> builder)
        {
            builder.ToTable("Leagues");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(l => l.Description)
                .HasMaxLength(500);

            builder.Property(l => l.TournamentId)
                .IsRequired();

            builder.Property(l => l.OwnerId)
                .IsRequired();

            builder.Property(l => l.InviteCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(l => l.IsPublic)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(l => l.IsGlobal)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(l => l.ImageUrl)
                .HasMaxLength(500);

            builder.Property(l => l.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(l => l.UpdatedAt)
                .IsRequired();

            // Unique constraints
            builder.HasIndex(l => l.InviteCode)
                .IsUnique();

            // Indexes
            builder.HasIndex(l => l.TournamentId);
            builder.HasIndex(l => l.OwnerId);

            // Relationships
            builder.HasOne(l => l.Tournament)
                .WithMany(t => t.Leagues)
                .HasForeignKey(l => l.TournamentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.Owner)
                .WithMany(u => u.OwnedLeagues)
                .HasForeignKey(l => l.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.Settings)
                .WithOne(ls => ls.League)
                .HasForeignKey<LeagueSettings>(ls => ls.LeagueId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.Members)
                .WithOne(lm => lm.League)
                .HasForeignKey(lm => lm.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.Predictions)
                .WithOne(p => p.League)
                .HasForeignKey(p => p.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.BonusPredictions)
                .WithOne(bp => bp.League)
                .HasForeignKey(bp => bp.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.Standings)
                .WithOne(ls => ls.League)
                .HasForeignKey(ls => ls.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.ChatMessages)
                .WithOne(cm => cm.League)
                .HasForeignKey(cm => cm.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
