using Application.Common;
using Application.Features.Admin.DTOs;
using MediatR;

namespace Application.Features.Admin.Leagues.Queries.GetAdminLeagueById
{
    public sealed record GetAdminLeagueByIdQuery(Guid LeagueId) : IRequest<Result<AdminLeagueDto>>;
}
