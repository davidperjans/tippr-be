using Domain.Common;

namespace Domain.Entities
{
    public class League : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public Guid TournamentId { get; set; }
        public Guid? OwnerId { get; set; }
        
        public string InviteCode { get; set; } = string.Empty;
        
        public bool IsPublic { get; set; } = false;
        public bool IsGlobal { get; set; } = false;
        public bool IsSystemCreated { get; set; } = false;
        
        public int? MaxMembers { get; set; }
        public string? ImageUrl { get; set; }


        // Navigation properties
        public Tournament Tournament { get; set; } = null!;
        public User? Owner { get; set; }

        public LeagueSettings Settings { get; set; } = null!;
        public ICollection<LeagueMember> Members { get; set; } = new List<LeagueMember>();
        public ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();
        public ICollection<BonusPrediction> BonusPredictions { get; set; } = new List<BonusPrediction>();
        public ICollection<LeagueStanding> Standings { get; set; } = new List<LeagueStanding>();
        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
}
