using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.DTOs;
using Application.Features.Tournaments.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Leagues.Queries.GetUserLeagues
{
    public class GetUserLeaguesQueryHandler : IRequestHandler<GetUserLeaguesQuery, Result<IReadOnlyList<LeagueListDto>>>
    {
        private readonly ITipprDbContext _db;
        public GetUserLeaguesQueryHandler(ITipprDbContext db)
        {
            _db = db;
        }
        public async Task<Result<IReadOnlyList<LeagueListDto>>> Handle(GetUserLeaguesQuery request, CancellationToken cancellationToken)
        {
            var userId = request.UserId;

            var list = await _db.LeagueMembers
                .AsNoTracking()
                .Where(lm => lm.UserId == userId)
                .OrderByDescending(lm => lm.JoinedAt)
                .Select(lm => lm.League)
                .Select(l => new LeagueListDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Description = l.Description,
                    TournamentId = l.TournamentId,
                    OwnerId = l.OwnerId,
                    InviteCode = l.InviteCode,
                    IsPublic = l.IsPublic,
                    IsGlobal = l.IsGlobal,
                    MaxMembers = l.MaxMembers,
                    ImageUrl = l.ImageUrl,

                    MemberCount = l.Members.Count(),

                    MyRank = l.Standings
                    .Where(s => s.UserId == userId)
                    .Select(s => (int?)s.Rank)
                    .FirstOrDefault() ?? 0,

                    MyTotalPoints = l.Standings
                    .Where(s => s.UserId == userId)
                    .Select(s => (int?)s.TotalPoints)
                    .FirstOrDefault() ?? 0
                })
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<LeagueListDto>>.Success(list);
        }
    }
}
