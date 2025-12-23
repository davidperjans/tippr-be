using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Users.Commands.UnbanUser
{
    public class UnbanUserCommandHandler : IRequestHandler<UnbanUserCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;

        public UnbanUserCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<bool>> Handle(UnbanUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result<bool>.NotFound("User not found", "admin.user_not_found");

            if (!user.IsBanned)
                return Result<bool>.BusinessRule("User is not banned", "admin.user_not_banned");

            user.IsBanned = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
