namespace API.Contracts.Leagues
{
    public sealed record CreateLeagueRequest(
        string Name,
        string? Description,
        Guid TournamentId,
        bool IsPublic,
        int? MaxMembers,
        string? ImageUrl
    );
}
