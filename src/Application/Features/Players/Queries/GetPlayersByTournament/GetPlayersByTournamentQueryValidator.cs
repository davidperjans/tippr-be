using FluentValidation;

namespace Application.Features.Players.Queries.GetPlayersByTournament
{
    public sealed class GetPlayersByTournamentQueryValidator : AbstractValidator<GetPlayersByTournamentQuery>
    {
        private static readonly string[] ValidPositions = { "Goalkeeper", "Defender", "Midfielder", "Attacker" };

        public GetPlayersByTournamentQueryValidator()
        {
            RuleFor(x => x.TournamentId).NotEmpty();

            RuleFor(x => x.Position)
                .Must(p => p == null || ValidPositions.Contains(p, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Position must be one of: Goalkeeper, Defender, Midfielder, Attacker");
        }
    }
}
