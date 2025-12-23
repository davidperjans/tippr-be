using Domain.Common;

namespace Domain.Entities
{
    public abstract class MatchSnapshotBase : BaseEntity
    {
        public Guid MatchId { get; set; }

        // Raw JSON från provider
        public string Json { get; set; } = string.Empty;

        public DateTime FetchedAt { get; set; } // UTC

        // Navigation
        public Match Match { get; set; } = null!;
    }

    public class MatchLineupSnapshot : MatchSnapshotBase { }
    public class MatchEventsSnapshot : MatchSnapshotBase { }
    public class MatchStatsSnapshot : MatchSnapshotBase { }
    public class MatchHeadToHeadSnapshot : MatchSnapshotBase { }
}
