using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Tournaments.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tournaments.Queries.GetTournamentById
{
    public class GetTournamentByIdQueryHandler : IRequestHandler<GetTournamentByIdQuery, Result<TournamentDto>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;
        public GetTournamentByIdQueryHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<Result<TournamentDto>> Handle(GetTournamentByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _db.Tournaments
                .AsNoTracking()
                .Where(x => x.Id == request.Id)
                .ProjectTo<TournamentDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (dto == null)
                return Result<TournamentDto>.NotFound("tournament not found", "tournament.not_found");

            return Result<TournamentDto>.Success(dto);
        }
    }
}
