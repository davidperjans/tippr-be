using Domain.Enums;

namespace Application.Features.Tournaments.DTOs
{
    public class TournamentDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int Year { get; init; }
        public TournamentType Type { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public string Country { get; init; } = string.Empty;
        public string? LogoUrl { get; init; }
        public bool IsActive { get; init; }
    }
}
