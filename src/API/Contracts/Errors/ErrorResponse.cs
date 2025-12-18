namespace API.Contracts.Errors
{
    public sealed class ErrorResponse
    {
        public string Type { get; set; } = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        public string Title { get; set; } = "An error occurred.";
        public int Status { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
