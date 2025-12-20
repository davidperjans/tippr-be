using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Commands.UploadAvatar
{
    public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, Result<string>>
    {
        private readonly ITipprDbContext _db;
        private readonly IAvatarStorage _storage;
        public UploadAvatarCommandHandler(ITipprDbContext db, IAvatarStorage storage)
        {
            _db = db;
            _storage = storage;
        }
        public async Task<Result<string>> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result<string>.NotFound("user not found", "user.not_found");

            await using var stream = request.File.OpenReadStream();

            var uploadResult = await _storage.UploadUserAvatarAsync(
                request.UserId,
                stream,
                request.File.ContentType,
                cancellationToken
            );

            if (!uploadResult.IsSuccess)
                return uploadResult;

            user.AvatarUrl = uploadResult.Data;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(uploadResult.Data!);
        }
    }
}
