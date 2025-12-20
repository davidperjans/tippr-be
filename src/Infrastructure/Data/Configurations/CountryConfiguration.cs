using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.ToTable("Countries");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.IsoCode)
                .IsRequired()
                .HasMaxLength(2);

            builder.HasIndex(x => x.IsoCode)
                .IsUnique();

            builder.Property(x => x.FlagUrl)
                .HasMaxLength(500);
        }
    }
}
