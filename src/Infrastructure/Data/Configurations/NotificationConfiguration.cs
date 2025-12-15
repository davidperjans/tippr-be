using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.UserId)
                .IsRequired();

            builder.Property(n => n.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(n => n.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(n => n.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(n => n.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(n => n.UserId);
            builder.HasIndex(n => n.IsRead);
            builder.HasIndex(n => n.CreatedAt);
            builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });

            // Relationships
            builder.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
