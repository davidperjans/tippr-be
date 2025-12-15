using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class LeagueSettingsConfiguration : IEntityTypeConfiguration<LeagueSettings>
    {
        public void Configure(EntityTypeBuilder<LeagueSettings> builder)
        {
            builder.ToTable("LeagueSettings");

            builder.HasKey(ls => ls.Id);

            builder.Property(ls => ls.LeagueId)
                .IsRequired();

            builder.Property(ls => ls.PredictionMode)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(ls => ls.DeadlineMinutes)
                .IsRequired()
                .HasDefaultValue(60);

            builder.Property(ls => ls.PointsCorrectScore)
                .IsRequired()
                .HasDefaultValue(7);

            builder.Property(ls => ls.PointsCorrectOutcome)
                .IsRequired()
                .HasDefaultValue(3);

            builder.Property(ls => ls.PointsCorrectGoals)
                .IsRequired()
                .HasDefaultValue(2);

            builder.Property(ls => ls.PointsRoundOf16Team)
                .IsRequired()
                .HasDefaultValue(2);

            builder.Property(ls => ls.PointsQuarterFinalTeam)
                .IsRequired()
                .HasDefaultValue(4);

            builder.Property(ls => ls.PointsSemiFinalTeam)
                .IsRequired()
                .HasDefaultValue(6);

            builder.Property(ls => ls.PointsFinalTeam)
                .IsRequired()
                .HasDefaultValue(8);

            builder.Property(ls => ls.PointsTopScorer)
                .IsRequired()
                .HasDefaultValue(20);

            builder.Property(ls => ls.PointsWinner)
                .IsRequired()
                .HasDefaultValue(20);

            builder.Property(ls => ls.PointsMostGoalsGroup)
                .IsRequired()
                .HasDefaultValue(10);

            builder.Property(ls => ls.PointsMostConcededGroup)
                .IsRequired()
                .HasDefaultValue(10);

            builder.Property(ls => ls.AllowLateEdits)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(ls => ls.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(ls => ls.UpdatedAt)
                .IsRequired();

            // Unique constraint - one settings per league
            builder.HasIndex(ls => ls.LeagueId)
                .IsUnique();

            // Relationship configured in LeagueConfiguration
        }
    }
}
