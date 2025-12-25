using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Players.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Players.Queries.GetPlayersByTeam
{
    public sealed class GetPlayersByTeamQueryHandler : IRequestHandler<GetPlayersByTeamQuery, Result<IReadOnlyList<PlayerDto>>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;

        // Define position order for sorting
        private static readonly Dictionary<string, int> PositionOrder = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Goalkeeper", 1 },
            { "Defender", 2 },
            { "Midfielder", 3 },
            { "Attacker", 4 }
        };

        public GetPlayersByTeamQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IReadOnlyList<PlayerDto>>> Handle(GetPlayersByTeamQuery request, CancellationToken ct)
        {
            var teamExists = await _db.Teams.AnyAsync(t => t.Id == request.TeamId, ct);
            if (!teamExists)
                return Result<IReadOnlyList<PlayerDto>>.NotFound("Team not found", "team.not_found");

            var players = await _db.Players
                .AsNoTracking()
                .Where(p => p.TeamId == request.TeamId)
                .ProjectTo<PlayerDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            // Sort by position order, then by number
            var sortedPlayers = players
                .OrderBy(p => PositionOrder.TryGetValue(p.Position ?? "", out var order) ? order : 99)
                .ThenBy(p => p.Number ?? 999)
                .ThenBy(p => p.Name)
                .ToList();

            return Result<IReadOnlyList<PlayerDto>>.Success(sortedPlayers);
        }
    }
}
