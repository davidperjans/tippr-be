using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class LeagueStandingConfiguration : IEntityTypeConfiguration<LeagueStanding>
    {
        public void Configure(EntityTypeBuilder<LeagueStanding> builder)
        {
            builder.ToTable("LeagueStandings");

            builder.HasKey(ls => ls.Id);

            builder.Property(ls => ls.LeagueId)
                .IsRequired();

            builder.Property(ls => ls.UserId)
                .IsRequired();

            builder.Property(ls => ls.TotalPoints)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(ls => ls.MatchPoints)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(ls => ls.BonusPoints)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(ls => ls.Rank)
                .IsRequired();

            builder.Property(ls => ls.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(ls => ls.UpdatedAt)
                .IsRequired();

            // Composite unique constraint - one standing per user per league
            builder.HasIndex(ls => new { ls.LeagueId, ls.UserId })
                .IsUnique();

            // Composite index for fast sorting by rank
            builder.HasIndex(ls => new { ls.LeagueId, ls.Rank });

            // Indexes
            builder.HasIndex(ls => ls.LeagueId);

            // Relationships
            builder.HasOne(ls => ls.League)
                .WithMany(l => l.Standings)
                .HasForeignKey(ls => ls.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ls => ls.User)
                .WithMany(u => u.LeagueStandings)
                .HasForeignKey(ls => ls.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
