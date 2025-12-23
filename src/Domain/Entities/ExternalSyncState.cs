using Domain.Common;

namespace Domain.Entities
{
    public class ExternalSyncState : BaseEntity
    {
        public Guid TournamentId { get; set; }

        // Ex: "ApiFootball"
        public string Provider { get; set; } = "ApiFootball";

        // Ex: "Teams", "Fixtures", "Results", "Lineups", "Events", "Stats"
        public string Resource { get; set; } = string.Empty;

        public DateTime LastSyncedAt { get; set; }
        public DateTime? NextAllowedSyncAt { get; set; }

        public string? LastHash { get; set; }
        public string? LastError { get; set; }

        // Navigation
        public Tournament Tournament { get; set; } = null!;
    }
}
