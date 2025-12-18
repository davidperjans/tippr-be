using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Teams.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Teams.Queries.GetTeam
{
    public sealed class GetTeamQueryHandler : IRequestHandler<GetTeamQuery, Result<TeamDto>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;

        public GetTeamQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<TeamDto>> Handle(GetTeamQuery request, CancellationToken ct)
        {
            var team = await _db.Teams
                .AsNoTracking()
                .Where(t => t.Id == request.Id)
                .ProjectTo<TeamDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(ct);

            if (team == null)
                return Result<TeamDto>.NotFound("team not found", "team.not_found");

            return Result<TeamDto>.Success(team);
        }
    }
}
