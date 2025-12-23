using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class TeamConfiguration : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.ToTable("Teams");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.TournamentId)
                .IsRequired();

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.DisplayName)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(t => t.Code)
                .HasMaxLength(3);

            builder.Property(t => t.LogoUrl)
                .HasMaxLength(500);

            builder.Property(t => t.GroupId)
                .IsRequired(false);

            builder.Property(t => t.FoundedYear)
                .IsRequired(false);

            builder.Property(t => t.FifaRank)
                .IsRequired(false);

            builder.Property(t => t.ApiFootballId)
                .IsRequired(false);

            builder.Property(t => t.VenueId)
                .IsRequired(false);

            builder.Property(t => t.FifaPoints)
                .IsRequired(false)
                .HasPrecision(10, 2);

            builder.Property(t => t.FifaRankingUpdatedAt)
                .IsRequired(false);

            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(t => t.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(t => t.TournamentId);
            builder.HasIndex(t => t.Code);

            // Relationships
            builder.HasOne(t => t.Tournament)
                .WithMany(tm => tm.Teams)
                .HasForeignKey(t => t.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.HomeMatches)
                .WithOne(m => m.HomeTeam)
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.AwayMatches)
                .WithOne(m => m.AwayTeam)
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Venue)
                .WithMany(v => v.Teams)
                .HasForeignKey(t => t.VenueId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
