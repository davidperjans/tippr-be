using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class LeagueMemberConfiguration : IEntityTypeConfiguration<LeagueMember>
    {
        public void Configure(EntityTypeBuilder<LeagueMember> builder)
        {
            builder.ToTable("LeagueMembers");

            builder.HasKey(lm => lm.Id);

            builder.Property(lm => lm.LeagueId)
                .IsRequired();

            builder.Property(lm => lm.UserId)
                .IsRequired();

            builder.Property(lm => lm.JoinedAt)
                .IsRequired();

            builder.Property(lm => lm.IsAdmin)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(lm => lm.IsMuted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(lm => lm.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(lm => lm.UpdatedAt)
                .IsRequired();

            // Composite unique constraint - user can only be member once per league
            builder.HasIndex(lm => new { lm.LeagueId, lm.UserId })
                .IsUnique();

            // Indexes
            builder.HasIndex(lm => lm.LeagueId);
            builder.HasIndex(lm => lm.UserId);

            // Relationships configured in UserConfiguration and LeagueConfiguration
        }
    }
}
