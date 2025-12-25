using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class BonusPredictionConfiguration : IEntityTypeConfiguration<BonusPrediction>
    {
        public void Configure(EntityTypeBuilder<BonusPrediction> builder)
        {
            builder.ToTable("BonusPredictions");

            builder.HasKey(bp => bp.Id);

            builder.Property(bp => bp.UserId)
                .IsRequired();

            builder.Property(bp => bp.BonusQuestionId)
                .IsRequired();

            builder.Property(bp => bp.LeagueId)
                .IsRequired();

            builder.Property(bp => bp.AnswerText)
                .HasMaxLength(200);

            builder.Property(bp => bp.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(bp => bp.UpdatedAt)
                .IsRequired();

            // Composite unique constraint - one answer per user per question per league
            builder.HasIndex(bp => new { bp.UserId, bp.BonusQuestionId, bp.LeagueId })
                .IsUnique();

            // Indexes
            builder.HasIndex(bp => bp.UserId);
            builder.HasIndex(bp => bp.BonusQuestionId);
            builder.HasIndex(bp => bp.LeagueId);

            // Relationships
            builder.HasOne(bp => bp.User)
                .WithMany(u => u.BonusPredictions)
                .HasForeignKey(bp => bp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(bp => bp.BonusQuestion)
                .WithMany(bq => bq.Predictions)
                .HasForeignKey(bp => bp.BonusQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(bp => bp.League)
                .WithMany(l => l.BonusPredictions)
                .HasForeignKey(bp => bp.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(bp => bp.AnswerTeam)
                .WithMany(t => t.BonusPredictions)
                .HasForeignKey(bp => bp.AnswerTeamId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(bp => bp.AnswerPlayer)
                .WithMany(p => p.BonusPredictions)
                .HasForeignKey(bp => bp.AnswerPlayerId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
