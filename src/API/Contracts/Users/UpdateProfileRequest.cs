namespace API.Contracts.Users
{
    public sealed record UpdateProfileRequest(
        string? DisplayName,
        string? Bio,
        Guid? FavoriteTeamId
    );
}
