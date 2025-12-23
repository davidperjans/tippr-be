using Domain.Enums;

namespace API.Contracts.Admin
{
    // User Requests
    public sealed record UpdateAdminUserRequest(
        string? Username,
        string? DisplayName,
        string? Email,
        string? Bio,
        string? AvatarUrl
    );

    public sealed record UpdateUserRoleRequest(UserRole Role);

    // League Requests
    public sealed record UpdateAdminLeagueRequest(
        string? Name,
        string? Description,
        bool? IsPublic,
        bool? IsGlobal,
        int? MaxMembers,
        string? ImageUrl
    );

    public sealed record AddLeagueMemberRequest(
        Guid UserId,
        bool IsAdmin = false
    );

    public sealed record UpdateLeagueMemberRequest(
        bool? IsAdmin,
        bool? IsMuted
    );

    // Tournament Requests
    public sealed record UpdateTournamentRequest(
        string? Name,
        int? Year,
        TournamentType? Type,
        DateTime? StartDate,
        DateTime? EndDate,
        string? LogoUrl
    );

    // Team Requests
    public sealed record CreateTeamRequest(
        Guid TournamentId,
        string Name,
        string Code,
        string? FlagUrl,
        string? GroupName,
        int? FifaRank,
        decimal? FifaPoints,
        int? ApiFootballId
    );

    public sealed record UpdateTeamRequest(
        string? Name,
        string? Code,
        string? FlagUrl,
        string? GroupName,
        int? FifaRank,
        decimal? FifaPoints,
        int? ApiFootballId
    );

    public sealed record BulkTeamRequest(
        string Name,
        string Code,
        string? FlagUrl,
        string? GroupName,
        int? FifaRank,
        decimal? FifaPoints,
        int? ApiFootballId
    );

    public sealed record BulkCreateTeamsRequest(
        Guid TournamentId,
        List<BulkTeamRequest> Teams
    );

    // Match Requests
    public sealed record CreateMatchRequest(
        Guid TournamentId,
        Guid HomeTeamId,
        Guid AwayTeamId,
        DateTime MatchDate,
        MatchStage Stage,
        string? Venue,
        int? ApiFootballId
    );

    public sealed record UpdateMatchRequest(
        DateTime? MatchDate,
        MatchStage? Stage,
        MatchStatus? Status,
        string? Venue,
        int? ApiFootballId
    );

    public sealed record BulkMatchRequest(
        Guid HomeTeamId,
        Guid AwayTeamId,
        DateTime MatchDate,
        MatchStage Stage,
        string? Venue,
        int? ApiFootballId
    );

    public sealed record BulkCreateMatchesRequest(
        Guid TournamentId,
        List<BulkMatchRequest> Matches
    );

    // Bonus Question Requests
    public sealed record UpdateBonusQuestionRequest(
        BonusQuestionType? QuestionType,
        string? Question,
        int? Points
    );
}
