using Domain.Common;

namespace Domain.Entities
{
    public class Team : BaseEntity
    {
        public Guid TournamentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? FlagUrl { get; set; }
        public string? GroupName { get; set; }
        public int? FifaRank { get; set; }
        public decimal? FifaPoints { get; set; }
        public DateTime? FifaRankingUpdatedAt { get; set; }
        public int? ApiFootballId { get; set; }

        // Navigation properties
        public Tournament Tournament { get; set; } = null!;
        public ICollection<Match> HomeMatches { get; set; } = new List<Match>();
        public ICollection<Match> AwayMatches { get; set; } = new List<Match>();
        public ICollection<User> FavoriteByUsers { get; set; } = new List<User>();
        public ICollection<BonusQuestion> BonusQuestionsAnswered { get; set; } = new List<BonusQuestion>();
        public ICollection<BonusPrediction> BonusPredictions { get; set; } = new List<BonusPrediction>();
    }
}
