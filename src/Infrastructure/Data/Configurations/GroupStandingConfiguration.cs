using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class GroupStandingConfiguration : IEntityTypeConfiguration<GroupStanding>
    {
        public void Configure(EntityTypeBuilder<GroupStanding> builder)
        {
            builder.ToTable("GroupStandings");

            builder.HasKey(gs => gs.Id);

            builder.Property(gs => gs.GroupId)
                .IsRequired();

            builder.Property(gs => gs.TeamId)
                .IsRequired();

            builder.Property(gs => gs.Position)
                .IsRequired();

            builder.Property(gs => gs.Played)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(gs => gs.Won)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(gs => gs.Drawn)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(gs => gs.Lost)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(gs => gs.GoalsFor)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(gs => gs.GoalsAgainst)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(gs => gs.GoalDifference)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(gs => gs.Points)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(gs => gs.Form)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(gs => gs.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(gs => gs.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(gs => gs.GroupId);
            builder.HasIndex(gs => gs.TeamId);
            builder.HasIndex(gs => new { gs.GroupId, gs.TeamId }).IsUnique();
            builder.HasIndex(gs => new { gs.GroupId, gs.Position });

            // Relationships
            builder.HasOne(gs => gs.Group)
                .WithMany(g => g.Standings)
                .HasForeignKey(gs => gs.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(gs => gs.Team)
                .WithMany()
                .HasForeignKey(gs => gs.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
