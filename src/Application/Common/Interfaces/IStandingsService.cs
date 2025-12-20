namespace Application.Common.Interfaces
{
    /// <summary>
    /// Enterprise-level service for managing league standings, points calculation, and rank assignment.
    /// This service ensures atomic updates to standings when match results or bonus questions are resolved.
    /// </summary>
    public interface IStandingsService
    {
        /// <summary>
        /// Scores all predictions for a match and updates standings for all affected leagues.
        /// Should be called when a match result is finalized or updated.
        /// </summary>
        /// <param name="matchId">The match to score predictions for</param>
        /// <param name="resultVersion">The result version to track which version was scored</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Number of predictions scored</returns>
        Task<int> ScorePredictionsForMatchAsync(Guid matchId, int resultVersion, CancellationToken ct = default);

        /// <summary>
        /// Awards points for all bonus predictions when a bonus question is resolved.
        /// Updates BonusPoints in standings for all affected leagues.
        /// </summary>
        /// <param name="bonusQuestionId">The resolved bonus question</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Number of predictions awarded points</returns>
        Task<int> ScoreBonusPredictionsAsync(Guid bonusQuestionId, CancellationToken ct = default);

        /// <summary>
        /// Recalculates all standings for a league from scratch based on all scored predictions.
        /// Useful for data integrity checks or when standings may have become inconsistent.
        /// </summary>
        /// <param name="leagueId">The league to recalculate</param>
        /// <param name="ct">Cancellation token</param>
        Task RecalculateStandingsForLeagueAsync(Guid leagueId, CancellationToken ct = default);

        /// <summary>
        /// Recalculates ranks for all members in a league based on current points.
        /// Handles tied ranks and updates PreviousRank for tracking rank changes.
        /// </summary>
        /// <param name="leagueId">The league to recalculate ranks for</param>
        /// <param name="ct">Cancellation token</param>
        Task RecalculateRanksForLeagueAsync(Guid leagueId, CancellationToken ct = default);

        /// <summary>
        /// Recalculates standings for all leagues in a tournament.
        /// Useful after bulk data imports or corrections.
        /// </summary>
        /// <param name="tournamentId">The tournament whose leagues should be recalculated</param>
        /// <param name="ct">Cancellation token</param>
        Task RecalculateStandingsForTournamentAsync(Guid tournamentId, CancellationToken ct = default);
    }
}
