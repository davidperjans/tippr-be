using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Teams.Commands.DeleteTeam
{
    public class DeleteTeamCommandHandler : IRequestHandler<DeleteTeamCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;

        public DeleteTeamCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<bool>> Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
        {
            var team = await _db.Teams
                .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken);

            if (team == null)
                return Result<bool>.NotFound("Team not found", "admin.team_not_found");

            // Check if team is used in any matches
            var hasMatches = await _db.Matches
                .AnyAsync(m => m.HomeTeamId == request.TeamId || m.AwayTeamId == request.TeamId, cancellationToken);

            if (hasMatches)
                return Result<bool>.BusinessRule("Cannot delete team that has matches", "admin.team_has_matches");

            _db.Teams.Remove(team);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
