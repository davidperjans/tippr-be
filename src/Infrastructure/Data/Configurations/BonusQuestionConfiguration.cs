using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class BonusQuestionConfiguration : IEntityTypeConfiguration<BonusQuestion>
    {
        public void Configure(EntityTypeBuilder<BonusQuestion> builder)
        {
            builder.ToTable("BonusQuestions");

            builder.HasKey(bq => bq.Id);

            builder.Property(bq => bq.TournamentId)
                .IsRequired();

            builder.Property(bq => bq.QuestionType)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(bq => bq.Question)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(bq => bq.AnswerText)
                .HasMaxLength(200);

            builder.Property(bq => bq.IsResolved)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(bq => bq.Points)
                .IsRequired();

            builder.Property(bq => bq.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(bq => bq.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(bq => bq.TournamentId);

            // Relationships
            builder.HasOne(bq => bq.Tournament)
                .WithMany(t => t.BonusQuestions)
                .HasForeignKey(bq => bq.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(bq => bq.AnswerTeam)
                .WithMany(t => t.BonusQuestionsAnswered)
                .HasForeignKey(bq => bq.AnswerTeamId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(bq => bq.Predictions)
                .WithOne(bp => bp.BonusQuestion)
                .HasForeignKey(bp => bp.BonusQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
