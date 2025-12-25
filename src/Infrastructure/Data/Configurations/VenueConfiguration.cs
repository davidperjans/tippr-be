using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class VenueConfiguration : IEntityTypeConfiguration<Venue>
    {
        public void Configure(EntityTypeBuilder<Venue> builder)
        {
            builder.ToTable("Venues");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.ApiFootballId)
                .IsRequired(false);

            builder.Property(v => v.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(v => v.Address)
                .HasMaxLength(300);

            builder.Property(v => v.City)
                .HasMaxLength(100);

            builder.Property(v => v.Capacity)
                .IsRequired(false);

            builder.Property(v => v.Surface)
                .HasMaxLength(50);

            builder.Property(v => v.ImageUrl)
                .HasMaxLength(500);

            builder.Property(v => v.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(v => v.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(v => v.ApiFootballId);
            builder.HasIndex(v => v.Name);

            // Relationships configured in TeamConfiguration and MatchConfiguration
        }
    }
}
