using Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
