using Domain.Common;

namespace Domain.Entities
{
    public class TournamentCountry
    {
        public Guid TournamentId { get; set; }
        public Tournament Tournament { get; set; } = null!;

        public Guid CountryId { get; set; }
        public Country Country { get; set; } = null!;
    }
}
