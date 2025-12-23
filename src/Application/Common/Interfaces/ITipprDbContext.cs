using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Common.Interfaces
{
    public interface ITipprDbContext
    {
        DbSet<Tournament> Tournaments { get; }
        DbSet<User> Users { get; }
        DbSet<League> Leagues { get; }
        DbSet<LeagueMember> LeagueMembers { get; }
        DbSet<LeagueSettings> LeagueSettings { get; }
        DbSet<LeagueStanding> LeagueStandings { get; }
        DbSet<Team> Teams { get; }
        DbSet<Group> Groups { get; }
        DbSet<GroupStanding> GroupStandings { get; }
        DbSet<Match> Matches { get; }
        DbSet<Prediction> Predictions { get; }
        DbSet<BonusQuestion> BonusQuestions { get; }
        DbSet<BonusPrediction> BonusPredictions { get; }
        DbSet<ChatMessage> ChatMessages { get; }
        DbSet<Notification> Notifications { get; }
        DbSet<Venue> Venues { get; }
        DbSet<Player> Players { get; }
        DbSet<ExternalSyncState> ExternalSyncStates { get; }
        DbSet<MatchLineupSnapshot> MatchLineupSnapshots { get; }
        DbSet<MatchEventsSnapshot> MatchEventsSnapshots { get; }
        DbSet<MatchStatsSnapshot> MatchStatsSnapshots { get; }
        DbSet<MatchHeadToHeadSnapshot> MatchHeadToHeadSnapshots { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    }
}
