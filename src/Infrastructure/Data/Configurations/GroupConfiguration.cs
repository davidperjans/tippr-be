using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.ToTable("Groups");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.TournamentId)
                .IsRequired();

            builder.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(10);  // "A", "B", etc.

            builder.Property(g => g.ApiFootballGroupId)
                .IsRequired(false);

            builder.Property(g => g.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(g => g.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(g => g.TournamentId);
            builder.HasIndex(g => new { g.TournamentId, g.Name }).IsUnique();

            // Relationships
            builder.HasOne(g => g.Tournament)
                .WithMany(t => t.Groups)
                .HasForeignKey(g => g.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(g => g.Teams)
                .WithOne(t => t.Group)
                .HasForeignKey(t => t.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(g => g.Standings)
                .WithOne(s => s.Group)
                .HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
