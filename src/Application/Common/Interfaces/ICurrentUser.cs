namespace Application.Common.Interfaces
{
    public interface ICurrentUser
    {
        Guid UserId { get; } // Throws if missing
        string Email { get; } // Throws if missing
        Guid AuthUserId { get; }
    }
}
