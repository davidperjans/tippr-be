using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.Admin.Leagues.Commands.RegenerateInviteCode
{
    public class RegenerateInviteCodeCommandHandler : IRequestHandler<RegenerateInviteCodeCommand, Result<string>>
    {
        private readonly ITipprDbContext _db;

        public RegenerateInviteCodeCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<string>> Handle(RegenerateInviteCodeCommand request, CancellationToken cancellationToken)
        {
            var league = await _db.Leagues
                .FirstOrDefaultAsync(l => l.Id == request.LeagueId, cancellationToken);

            if (league == null)
                return Result<string>.NotFound("League not found", "admin.league_not_found");

            league.InviteCode = await GenerateUniqueInviteCodeAsync(cancellationToken);
            league.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(league.InviteCode);
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
