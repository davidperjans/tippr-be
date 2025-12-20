using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public sealed class TournamentCountryConfiguration : IEntityTypeConfiguration<TournamentCountry>
    {
        public void Configure(EntityTypeBuilder<TournamentCountry> builder)
        {
            builder.ToTable("TournamentCountries");

            // Composite PK
            builder.HasKey(x => new { x.TournamentId, x.CountryId });

            builder.HasOne(x => x.Tournament)
                .WithMany(t => t.Countries)
                .HasForeignKey(x => x.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Country)
                .WithMany(c => c.Tournaments)
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CountryId);
        }
    }
}
