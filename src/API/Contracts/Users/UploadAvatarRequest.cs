namespace API.Contracts.Users
{
    public sealed class UploadAvatarRequest
    {
        public IFormFile File { get; set; } = default!;
    }
}
