using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.AuthUserId)
                .IsRequired();

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

            // Custom converter to handle both "1"/"0" (integer strings) and "Admin"/"User" (enum names)
            var roleConverter = new ValueConverter<UserRole, string>(
                v => v.ToString(), // Write: enum -> "Admin" or "User"
                v => ParseRole(v)  // Read: "1", "0", "Admin", "User" -> enum
            );

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion(roleConverter);

            builder.Property(u => u.IsBanned)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(u => u.LastLoginAt)
                .IsRequired(false);

            builder.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(u => u.UpdatedAt)
                .IsRequired();

            // Unique constraints
            builder.HasIndex(u => u.Username)
                .IsUnique();

            builder.HasIndex(u => u.Email);

            builder.HasIndex(u => u.AuthUserId)
                .IsUnique()
                .HasDatabaseName("IX_Users_AuthUserId");

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

        /// <summary>
        /// Parses role from database string value.
        /// Handles both integer strings ("0", "1") and enum names ("User", "Admin").
        /// </summary>
        private static UserRole ParseRole(string value)
        {
            // Try parse as enum name first (e.g., "Admin", "User")
            if (Enum.TryParse<UserRole>(value, ignoreCase: true, out var role))
                return role;

            // Try parse as integer string (e.g., "1", "0")
            if (int.TryParse(value, out var intValue) && Enum.IsDefined(typeof(UserRole), intValue))
                return (UserRole)intValue;

            // Default to User if unknown
            return UserRole.User;
        }
    }
}
