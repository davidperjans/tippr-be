using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Users.Commands.BanUser
{
    public class BanUserCommandHandler : IRequestHandler<BanUserCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;

        public BanUserCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<bool>> Handle(BanUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result<bool>.NotFound("User not found", "admin.user_not_found");

            if (user.IsBanned)
                return Result<bool>.BusinessRule("User is already banned", "admin.user_already_banned");

            user.IsBanned = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
