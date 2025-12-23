using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Leagues.Queries.GetAdminLeagueMembers
{
    public class GetAdminLeagueMembersQueryHandler : IRequestHandler<GetAdminLeagueMembersQuery, Result<IReadOnlyList<AdminLeagueMemberDto>>>
    {
        private readonly ITipprDbContext _db;

        public GetAdminLeagueMembersQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<IReadOnlyList<AdminLeagueMemberDto>>> Handle(GetAdminLeagueMembersQuery request, CancellationToken cancellationToken)
        {
            var leagueExists = await _db.Leagues.AnyAsync(l => l.Id == request.LeagueId, cancellationToken);
            if (!leagueExists)
                return Result<IReadOnlyList<AdminLeagueMemberDto>>.NotFound("League not found", "admin.league_not_found");

            var members = await _db.LeagueMembers
                .AsNoTracking()
                .Where(lm => lm.LeagueId == request.LeagueId)
                .Select(lm => new AdminLeagueMemberDto
                {
                    Id = lm.Id,
                    LeagueId = lm.LeagueId,
                    UserId = lm.UserId,
                    Username = lm.User.Username,
                    DisplayName = lm.User.DisplayName,
                    Email = lm.User.Email,
                    AvatarUrl = lm.User.AvatarUrl,
                    JoinedAt = lm.JoinedAt,
                    IsAdmin = lm.IsAdmin,
                    IsMuted = lm.IsMuted,
                    TotalPoints = _db.LeagueStandings
                        .Where(ls => ls.LeagueId == lm.LeagueId && ls.UserId == lm.UserId)
                        .Select(ls => ls.TotalPoints)
                        .FirstOrDefault(),
                    Rank = _db.LeagueStandings
                        .Where(ls => ls.LeagueId == lm.LeagueId && ls.UserId == lm.UserId)
                        .Select(ls => ls.Rank)
                        .FirstOrDefault()
                })
                .OrderBy(m => m.JoinedAt)
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<AdminLeagueMemberDto>>.Success(members);
        }
    }
}
