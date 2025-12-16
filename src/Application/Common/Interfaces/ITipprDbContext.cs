using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces
{
    public interface ITipprDbContext
    {
        DbSet<Tournament> Tournaments { get; }
        DbSet<User> Users { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
