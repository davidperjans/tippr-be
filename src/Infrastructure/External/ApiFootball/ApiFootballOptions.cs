namespace Infrastructure.External.ApiFootball
{
    public sealed class ApiFootballOptions
    {
        public string BaseUrl { get; set; } = "https://v3.football.api-sports.io";
        public string ApiKey { get; set; } = string.Empty;
    }
}
