using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Admin.Chat.Commands.DeleteChatMessage
{
    public class DeleteChatMessageCommandHandler : IRequestHandler<DeleteChatMessageCommand, Result<bool>>
    {
        private readonly ITipprDbContext _db;

        public DeleteChatMessageCommandHandler(ITipprDbContext db)
        {
            _db = db;
        }

        public async Task<Result<bool>> Handle(DeleteChatMessageCommand request, CancellationToken cancellationToken)
        {
            var message = await _db.ChatMessages
                .FirstOrDefaultAsync(cm => cm.Id == request.MessageId, cancellationToken);

            if (message == null)
                return Result<bool>.NotFound("Message not found", "admin.message_not_found");

            // Soft delete
            message.IsDeleted = true;
            message.Message = "[Message deleted by admin]";
            message.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
