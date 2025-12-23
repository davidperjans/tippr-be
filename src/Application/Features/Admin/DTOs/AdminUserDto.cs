using Domain.Enums;

namespace Application.Features.Admin.DTOs
{
    public class AdminUserDto
    {
        public Guid Id { get; init; }
        public Guid AuthUserId { get; init; }
        public string Username { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? AvatarUrl { get; init; }
        public string? Bio { get; init; }
        public UserRole Role { get; init; }
        public bool IsBanned { get; init; }
        public Guid? FavoriteTeamId { get; init; }
        public string? FavoriteTeamName { get; init; }
        public DateTime? LastLoginAt { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public int LeagueCount { get; init; }
        public int OwnedLeagueCount { get; init; }
        public int PredictionCount { get; init; }
    }

    public class AdminUserListDto
    {
        public Guid Id { get; init; }
        public string Username { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? AvatarUrl { get; init; }
        public UserRole Role { get; init; }
        public bool IsBanned { get; init; }
        public DateTime? LastLoginAt { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = new List<T>();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}
