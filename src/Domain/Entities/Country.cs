using Domain.Common;

namespace Domain.Entities
{
    public class Country : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string IsoCode { get; set; } = null!; // "US", "CA"
        public string FlagUrl { get; set; } = null!;

        public ICollection<TournamentCountry> Tournaments { get; set; } = new List<TournamentCountry>();
    }
}
