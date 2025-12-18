using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Leagues.DTOs;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Leagues.Commands.UpdateLeagueSettings
{
    public sealed class UpdateLeagueSettingsCommandHandler : IRequestHandler<UpdateLeagueSettingsCommand, Result<LeagueSettingsDto>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;
        public UpdateLeagueSettingsCommandHandler(ITipprDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<Result<LeagueSettingsDto>> Handle(UpdateLeagueSettingsCommand request, CancellationToken ct)
        {
            var league = await _db.Leagues
                .Include(l => l.Settings)
                .FirstOrDefaultAsync(l => l.Id == request.LeagueId, ct);

            if (league == null)
                return Result<LeagueSettingsDto>.NotFound("league not found.", "league.not_found");

            if (league.OwnerId != request.UserId)
                return Result<LeagueSettingsDto>.Forbidden("only the league owner can update settings.", "league.forbidden");

            if (league.Settings == null)
            {
                league.Settings = new LeagueSettings
                {
                    Id = Guid.NewGuid(),
                    LeagueId = league.Id,
                    CreatedAt = DateTime.UtcNow,
                };
                _db.LeagueSettings.Add(league.Settings);
            }

            _mapper.Map(request, league.Settings);
            league.Settings.UpdatedAt = DateTime.UtcNow;
            league.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            var dto = _mapper.Map<LeagueSettingsDto>(league.Settings);

            return Result<LeagueSettingsDto>.Success(dto);
        }
    }
}
