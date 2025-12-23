using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class MatchConfiguration : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> builder)
        {
            builder.ToTable("Matches");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.TournamentId)
                .IsRequired();

            builder.Property(m => m.HomeTeamId)
                .IsRequired();

            builder.Property(m => m.AwayTeamId)
                .IsRequired();

            builder.Property(m => m.MatchDate)
                .IsRequired();

            builder.Property(m => m.Stage)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(m => m.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(m => m.VenueId)
                .IsRequired(false);

            builder.Property(m => m.VenueName)
                .HasMaxLength(200);

            builder.Property(m => m.VenueCity)
                .HasMaxLength(100);

            builder.Property(m => m.ApiFootballId)
                .IsRequired(false);

            builder.Property(m => m.ResultVersion)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(m => m.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(m => m.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(m => m.TournamentId);
            builder.HasIndex(m => m.MatchDate);
            builder.HasIndex(m => m.Status);

            // Composite index for common queries
            builder.HasIndex(m => new { m.TournamentId, m.Status, m.MatchDate });

            // Relationships
            builder.HasOne(m => m.Tournament)
                .WithMany(t => t.Matches)
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.HomeTeam)
                .WithMany(t => t.HomeMatches)
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.AwayTeam)
                .WithMany(t => t.AwayMatches)
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Venue)
                .WithMany(v => v.Matches)
                .HasForeignKey(m => m.VenueId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(m => m.Predictions)
                .WithOne(p => p.Match)
                .HasForeignKey(p => p.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
