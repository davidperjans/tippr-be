namespace Application.Features.Leagues.DTOs
{
    public sealed class LeagueStandingDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }

        public int Rank { get; set; }
        public int? PreviousRank { get; set; }

        /// <summary>
        /// Rank change from previous calculation.
        /// Positive = moved up (e.g., +3 means from 5th to 2nd)
        /// Negative = moved down (e.g., -2 means from 3rd to 5th)
        /// Zero = no change
        /// Null = no previous rank (new member)
        /// </summary>
        public int? RankChange { get; set; }

        public int TotalPoints { get; set; }
        public int MatchPoints { get; set; }
        public int BonusPoints { get; set; }
    }
}
