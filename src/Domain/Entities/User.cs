using Domain.Common;

namespace Domain.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public Guid? FavoriteTeamId { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public Team? FavoriteTeam { get; set; }
        public ICollection<League> OwnedLeagues { get; set; } = new List<League>();
        public ICollection<LeagueMember> LeagueMemberships { get; set; } = new List<LeagueMember>();
        public ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();
        public ICollection<BonusPrediction> BonusPredictions { get; set; } = new List<BonusPrediction>();
        public ICollection<LeagueStanding> LeagueStandings { get; set; } = new List<LeagueStanding>();
        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
