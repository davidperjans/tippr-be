using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
    {
        public void Configure(EntityTypeBuilder<Tournament> builder)
        {
            builder.ToTable("Tournaments");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Year)
                .IsRequired();

            builder.Property(t => t.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(t => t.StartDate)
                .IsRequired();

            builder.Property(t => t.EndDate)
                .IsRequired();

            builder.Property(t => t.LogoUrl)
                .HasMaxLength(500);

            builder.Property(t => t.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(t => t.ApiFootballLeagueId)
                .IsRequired(false);

            builder.Property(t => t.ApiFootballSeason)
                .IsRequired(false);

            builder.Property(t => t.ApiFootballEnabled)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(t => t.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasMany(t => t.Teams)
                .WithOne(tm => tm.Tournament)
                .HasForeignKey(tm => tm.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Matches)
                .WithOne(m => m.Tournament)
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.BonusQuestions)
                .WithOne(bq => bq.Tournament)
                .HasForeignKey(bq => bq.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Leagues)
                .WithOne(l => l.Tournament)
                .HasForeignKey(l => l.TournamentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
