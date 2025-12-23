using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Leagues.Commands.UpdateLeagueMember
{
    public class UpdateLeagueMemberCommandHandler : IRequestHandler<UpdateLeagueMemberCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;

        public UpdateLeagueMemberCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<bool>> Handle(UpdateLeagueMemberCommand request, CancellationToken cancellationToken)
        {
            var member = await _db.LeagueMembers
                .FirstOrDefaultAsync(lm => lm.LeagueId == request.LeagueId && lm.UserId == request.UserId, cancellationToken);

            if (member == null)
                return Result<bool>.NotFound("Member not found in this league", "admin.member_not_found");

            if (request.IsAdmin.HasValue)
                member.IsAdmin = request.IsAdmin.Value;

            if (request.IsMuted.HasValue)
                member.IsMuted = request.IsMuted.Value;

            await _db.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
