namespace Application.Features.Players.DTOs
{
    public sealed class PlayerDto
    {
        public Guid Id { get; init; }
        public Guid TeamId { get; init; }

        public string Name { get; init; } = string.Empty;
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public int? Number { get; init; }
        public string? Position { get; init; }
        public string? PhotoUrl { get; init; }

        public DateTime? DateOfBirth { get; init; }
        public int? Age { get; init; }
        public string? Nationality { get; init; }
        public int? Height { get; init; }
        public int? Weight { get; init; }
        public bool? Injured { get; init; }

        public int? ApiFootballId { get; init; }
    }

    public sealed class PlayerWithTeamDto
    {
        public Guid Id { get; init; }
        public Guid TeamId { get; init; }
        public string TeamName { get; init; } = string.Empty;
        public string? TeamDisplayName { get; init; }
        public string? TeamLogoUrl { get; init; }

        public string Name { get; init; } = string.Empty;
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public int? Number { get; init; }
        public string? Position { get; init; }
        public string? PhotoUrl { get; init; }

        public DateTime? DateOfBirth { get; init; }
        public int? Age { get; init; }
        public string? Nationality { get; init; }
        public int? Height { get; init; }
        public int? Weight { get; init; }
        public bool? Injured { get; init; }

        public int? ApiFootballId { get; init; }
    }
}
