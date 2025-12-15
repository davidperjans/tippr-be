using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("ChatMessages");

            builder.HasKey(cm => cm.Id);

            builder.Property(cm => cm.LeagueId)
                .IsRequired();

            builder.Property(cm => cm.UserId)
                .IsRequired();

            builder.Property(cm => cm.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(cm => cm.IsEdited)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(cm => cm.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(cm => cm.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(cm => cm.UpdatedAt)
                .IsRequired();

            // Indexes for pagination and chronological sorting
            builder.HasIndex(cm => cm.LeagueId);
            builder.HasIndex(cm => cm.CreatedAt);
            builder.HasIndex(cm => new { cm.LeagueId, cm.CreatedAt });

            // Relationships
            builder.HasOne(cm => cm.League)
                .WithMany(l => l.ChatMessages)
                .HasForeignKey(cm => cm.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cm => cm.User)
                .WithMany(u => u.ChatMessages)
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
