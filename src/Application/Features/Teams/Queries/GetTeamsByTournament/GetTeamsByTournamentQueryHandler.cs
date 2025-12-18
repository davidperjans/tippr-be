using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Teams.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Teams.Queries.GetTeamsByTournament
{
    public sealed class GetTeamsByTournamentQueryHandler : IRequestHandler<GetTeamsByTournamentQuery, Result<IReadOnlyList<TeamDto>>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;

        public GetTeamsByTournamentQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IReadOnlyList<TeamDto>>> Handle(GetTeamsByTournamentQuery request, CancellationToken ct)
        {
            if (request.TournamentId == Guid.Empty)
                return Result<IReadOnlyList<TeamDto>>.Failure("tournamentId is required.");

            var teams = await _db.Teams
                .AsNoTracking()
                .Where(t => t.TournamentId == request.TournamentId)
                .OrderBy(t => t.Name)
                .ProjectTo<TeamDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);

            return Result<IReadOnlyList<TeamDto>>.Success(teams);
        }
    }
}
