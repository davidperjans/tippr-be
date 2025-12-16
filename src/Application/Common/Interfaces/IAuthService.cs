using Domain.Entities;

namespace Application.Common.Interfaces
{
    public interface IAuthService
    {
        Task<User> GetOrCreateUserAsync(Guid authUserId, string email, CancellationToken cancellationToken = default);

        Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<User?> GetByAuthUserIdAsync(Guid authUserId, CancellationToken cancellationToken = default);

        Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
