namespace Application.Features.Venues.DTOs
{
    public sealed class VenueDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Address { get; init; }
        public string? City { get; init; }
        public int? Capacity { get; init; }
        public string? Surface { get; init; }
        public string? ImageUrl { get; init; }
        public int? ApiFootballId { get; init; }
    }
}
