using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Players.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Players.Queries.GetPlayersByTournament
{
    public sealed class GetPlayersByTournamentQueryHandler
        : IRequestHandler<GetPlayersByTournamentQuery, Result<IReadOnlyList<PlayerWithTeamDto>>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;

        private static readonly Dictionary<string, int> PositionOrder = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Goalkeeper", 1 },
            { "Defender", 2 },
            { "Midfielder", 3 },
            { "Attacker", 4 }
        };

        public GetPlayersByTournamentQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IReadOnlyList<PlayerWithTeamDto>>> Handle(
            GetPlayersByTournamentQuery request,
            CancellationToken ct)
        {
            var tournamentExists = await _db.Tournaments.AnyAsync(t => t.Id == request.TournamentId, ct);
            if (!tournamentExists)
                return Result<IReadOnlyList<PlayerWithTeamDto>>.NotFound("Tournament not found", "tournament.not_found");

            var query = _db.Players
                .AsNoTracking()
                .Include(p => p.Team)
                .Where(p => p.Team.TournamentId == request.TournamentId);

            // Filter by position if specified
            if (!string.IsNullOrWhiteSpace(request.Position))
            {
                query = query.Where(p => p.Position != null &&
                    p.Position.ToLower() == request.Position.ToLower());
            }

            // Search by name if specified
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchLower) ||
                    (p.FirstName != null && p.FirstName.ToLower().Contains(searchLower)) ||
                    (p.LastName != null && p.LastName.ToLower().Contains(searchLower)));
            }

            var players = await query
                .ProjectTo<PlayerWithTeamDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            // Sort by team name, then position, then number
            var sortedPlayers = players
                .OrderBy(p => p.TeamDisplayName ?? p.TeamName)
                .ThenBy(p => PositionOrder.TryGetValue(p.Position ?? "", out var order) ? order : 99)
                .ThenBy(p => p.Number ?? 999)
                .ThenBy(p => p.Name)
                .ToList();

            return Result<IReadOnlyList<PlayerWithTeamDto>>.Success(sortedPlayers);
        }
    }
}
