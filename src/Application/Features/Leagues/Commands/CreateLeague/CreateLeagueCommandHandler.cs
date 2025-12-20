using Application.Common;
using Application.Common.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.Leagues.Commands.CreateLeague
{
    public class CreateLeagueCommandHandler : IRequestHandler<CreateLeagueCommand, Result<Guid>>
    {
        private readonly ITipprDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly IStandingsService _standingsService;
        public CreateLeagueCommandHandler(ITipprDbContext db, IMapper mapper, ICurrentUser currentUser, IStandingsService standingsService)
        {
            _db = db;
            _mapper = mapper;
            _currentUser = currentUser;
            _standingsService = standingsService;
        }
        public async Task<Result<Guid>> Handle(CreateLeagueCommand request, CancellationToken cancellationToken)
        {
            var ownerId = _currentUser.UserId;

            var exists = await _db.Leagues.AnyAsync(l => l.Name == request.Name && l.OwnerId == ownerId, cancellationToken);

            if (exists)
                return Result<Guid>.Conflict("league already exists", "league.already_exists");

            var entity = _mapper.Map<League>(request);

            entity.Id = Guid.NewGuid();
            entity.OwnerId = ownerId;
            entity.IsGlobal = false;
            entity.IsSystemCreated = false;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            entity.InviteCode = await GenerateUniqueInviteCodeAsync(cancellationToken);
            

            entity.Settings = new LeagueSettings
            {
                Id = Guid.NewGuid(),
                LeagueId = entity.Id,
                PredictionMode = PredictionMode.AllAtOnce,
                DeadlineMinutes = 60,
                PointsCorrectScore = 7,
                PointsCorrectOutcome = 3,
                PointsCorrectGoals = 2,
                PointsRoundOf16Team = 2,
                PointsQuarterFinalTeam = 4,
                PointsSemiFinalTeam = 6,
                PointsFinalTeam = 8,
                PointsTopScorer = 20,
                PointsWinner = 20,
                PointsMostGoalsGroup = 10,
                PointsMostConcededGroup = 10,
                AllowLateEdits = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.LeagueMembers.Add(new LeagueMember
            {
                Id = Guid.NewGuid(),
                LeagueId = entity.Id,
                UserId = ownerId,
                JoinedAt = DateTime.UtcNow,
                IsAdmin = true,
                IsMuted = false
            });

            _db.LeagueStandings.Add(new LeagueStanding
            {
                Id = Guid.NewGuid(),
                LeagueId = entity.Id,
                UserId = ownerId,
                TotalPoints = 0,
                MatchPoints = 0,
                BonusPoints = 0,
                Rank = 1,
                PreviousRank = null,
                UpdatedAt = DateTime.UtcNow
            });

            _db.Leagues.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            await _standingsService.RecalculateRanksForLeagueAsync(entity.Id, cancellationToken);

            return Result<Guid>.Success(entity.Id);
        }

        private async Task<string> GenerateUniqueInviteCodeAsync(CancellationToken ct)
        {
            const int length = 8;

            while (true)
            {
                var code = GenerateCode(length);

                var exists = await _db.Leagues.AsNoTracking().AnyAsync(l => l.InviteCode == code, ct);

                if (!exists)
                    return code;
            }
        }

        private static string GenerateCode(int length)
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            var bytes = RandomNumberGenerator.GetBytes(length);
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                sb.Append(alphabet[bytes[i] % alphabet.Length]);
            }

            return sb.ToString();
        }
    }
}
