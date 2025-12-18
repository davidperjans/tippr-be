using Domain.Enums;

namespace API.Contracts.Matches
{
    public sealed record UpdateMatchResultRequest(
        int? HomeScore, 
        int? AwayScore, 
        MatchStatus Status
    );
}
