using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired();

            builder.Property(u => u.AvatarUrl)
                .HasMaxLength(500);

            builder.Property(u => u.Bio)
                .HasMaxLength(500);

            builder.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(u => u.UpdatedAt)
                .IsRequired();

            // Unique constraints
            builder.HasIndex(u => u.Username)
                .IsUnique();

            builder.HasIndex(u => u.Email);

            // Relationships
            builder.HasOne(u => u.FavoriteTeam)
                .WithMany(t => t.FavoriteByUsers)
                .HasForeignKey(u => u.FavoriteTeamId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(u => u.OwnedLeagues)
                .WithOne(l => l.Owner)
                .HasForeignKey(l => l.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.LeagueMemberships)
                .WithOne(lm => lm.User)
                .HasForeignKey(lm => lm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Predictions)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.BonusPredictions)
                .WithOne(bp => bp.User)
                .HasForeignKey(bp => bp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.LeagueStandings)
                .WithOne(ls => ls.User)
                .HasForeignKey(ls => ls.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.ChatMessages)
                .WithOne(cm => cm.User)
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
