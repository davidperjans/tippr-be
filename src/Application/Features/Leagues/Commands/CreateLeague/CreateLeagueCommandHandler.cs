using Application.Common;
using Application.Common.Interfaces;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Leagues.Commands.CreateLeague
{
    public class CreateLeagueCommandHandler : IRequestHandler<CreateLeagueCommand, Result<Guid>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;
        public CreateLeagueCommandHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<Result<Guid>> Handle(CreateLeagueCommand request, CancellationToken cancellationToken)
        {
            var exists = await _db.Leagues.AnyAsync(l => l.Name == request.Name && l.OwnerId == request.UserId, cancellationToken);

            if (exists)
                return Result<Guid>.Failure("league already exists");

            var entity = _mapper.Map<League>(request);

            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;

            _db.Leagues.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(entity.Id);
        }
    }
}
