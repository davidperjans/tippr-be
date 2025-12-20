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
        DbSet<Match> Matches { get; }
        DbSet<Prediction> Predictions { get; }
        DbSet<BonusQuestion> BonusQuestions { get; }
        DbSet<BonusPrediction> BonusPredictions { get; }
        DbSet<ChatMessage> ChatMessages { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    }
}
