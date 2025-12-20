namespace Application.Common.Interfaces
{
    public interface IAvatarStorage
    {
        Task<Result<string>> UploadUserAvatarAsync(Guid userId, Stream file, string contentType, CancellationToken ct);
    }
}
