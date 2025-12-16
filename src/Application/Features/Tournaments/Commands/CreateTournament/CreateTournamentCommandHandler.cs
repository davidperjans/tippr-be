using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Tournaments.DTOs;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Tournaments.Commands.CreateTournament
{
    public class CreateTournamentCommandHandler : IRequestHandler<CreateTournamentCommand, Result<Guid>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;
        public CreateTournamentCommandHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<Result<Guid>> Handle(CreateTournamentCommand request, CancellationToken cancellationToken)
        {
            var exists = await _db.Tournaments
                .AnyAsync(t => t.Name == request.Name && t.Year == request.Year, cancellationToken);

            if (exists)
                return Result<Guid>.Failure("tournament already exists");

            var entity = _mapper.Map<Tournament>(request);

            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;

            _db.Tournaments.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(entity.Id);
        }
    }
}
