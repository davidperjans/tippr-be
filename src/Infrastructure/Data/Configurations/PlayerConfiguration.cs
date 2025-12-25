using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> builder)
        {
            builder.ToTable("Players");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.TeamId)
                .IsRequired();

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.FirstName)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(p => p.LastName)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(p => p.Number)
                .IsRequired(false);

            builder.Property(p => p.Position)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(p => p.PhotoUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(p => p.DateOfBirth)
                .IsRequired(false);

            builder.Property(p => p.Age)
                .IsRequired(false);

            builder.Property(p => p.Nationality)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(p => p.Height)
                .IsRequired(false);

            builder.Property(p => p.Weight)
                .IsRequired(false);

            builder.Property(p => p.Injured)
                .IsRequired(false);

            builder.Property(p => p.ApiFootballId)
                .IsRequired(false);

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(p => p.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(p => p.TeamId);
            builder.HasIndex(p => p.ApiFootballId);
            builder.HasIndex(p => new { p.TeamId, p.ApiFootballId }).IsUnique();

            // Relationships
            builder.HasOne(p => p.Team)
                .WithMany(t => t.Players)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
