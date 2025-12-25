using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Players.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Players.Queries.GetPlayer
{
    public sealed class GetPlayerQueryHandler : IRequestHandler<GetPlayerQuery, Result<PlayerWithTeamDto>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;

        public GetPlayerQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<PlayerWithTeamDto>> Handle(GetPlayerQuery request, CancellationToken ct)
        {
            var player = await _db.Players
                .AsNoTracking()
                .Include(p => p.Team)
                .Where(p => p.Id == request.Id)
                .ProjectTo<PlayerWithTeamDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(ct);

            if (player == null)
                return Result<PlayerWithTeamDto>.NotFound("Player not found", "player.not_found");

            return Result<PlayerWithTeamDto>.Success(player);
        }
    }
}
