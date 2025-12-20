using Application.Common;
using Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Supabase;
using System.Net;
using System.Net.Http.Headers;

namespace Infrastructure.Services
{
    public sealed class SupabaseAvatarStorage : IAvatarStorage
    {
        private readonly HttpClient _http;
        private readonly SupabaseStorageOptions _opt;
        private const string Bucket = "avatars";

        public SupabaseAvatarStorage(HttpClient http, IOptions<SupabaseStorageOptions> opt)
        {
            _http = http;
            _opt = opt.Value;
        }

        public async Task<Result<string>> UploadUserAvatarAsync(
            Guid userId,
            Stream file,
            string contentType,
            CancellationToken ct)
        {
            var objectPath = $"{userId}/avatar.png";
            var uploadUrl = $"{_opt.Url}/storage/v1/object/{Bucket}/{objectPath}";
            var publicUrl = $"{_opt.Url}/storage/v1/object/public/{Bucket}/{objectPath}";

            try
            {
                using var content = new StreamContent(file);
                content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                using var req = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
                {
                    Content = content
                };

                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opt.ServiceKey);
                req.Headers.Add("apikey", _opt.ServiceKey);
                req.Headers.Add("x-upsert", "true"); // writes over old file to save storage

                using var res = await _http.SendAsync(req, ct);

                if (res.IsSuccessStatusCode)
                    return Result<string>.Success(publicUrl);

                var body = await res.Content.ReadAsStringAsync(ct);

                if (res.StatusCode == HttpStatusCode.Unauthorized)
                    return Result<string>.Unauthorized("Supabase storage unauthorized", "storage.unauthorized");

                if (res.StatusCode == HttpStatusCode.Forbidden)
                    return Result<string>.Forbidden("Supabase storage forbidden", "storage.forbidden");

                return Result<string>.Failure($"Supabase storage upload failed ({(int)res.StatusCode})", "avatar.upload_failed");
            }
            catch (TaskCanceledException)
            {
                return Result<string>.Failure("Storage upload timed out", "avatar.upload_timeout");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(ex.Message, "avatar.upload_exception");
            }
        }
    }

    public sealed class SupabaseStorageOptions
    {
        public string Url { get; set; } = string.Empty;
        public string ServiceKey { get; set; } = string.Empty;
    }
}
