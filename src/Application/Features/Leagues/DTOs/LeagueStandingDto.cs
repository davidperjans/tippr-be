namespace Application.Features.Leagues.DTOs
{
    public sealed class LeagueStandingDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;

        public int Rank { get; set; }
        public int TotalPoints { get; set; }
        public int MatchPoints { get; set; }
        public int BonusPoints { get; set; }
    }
}
