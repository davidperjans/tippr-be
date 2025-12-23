using Application.Common;
using Application.Features.Admin.DTOs;
using MediatR;

namespace Application.Features.Admin.Leagues.Queries.GetAdminLeagues
{
    public sealed record GetAdminLeaguesQuery(
        Guid? TournamentId,
        Guid? OwnerId,
        string? Search,
        bool? IsPublic,
        int Page = 1,
        int PageSize = 20
    ) : IRequest<Result<PagedResult<AdminLeagueListDto>>>;
}
