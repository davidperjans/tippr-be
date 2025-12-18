using Application.Common.Interfaces;
using Domain.Entities;

namespace Infrastructure.Services
{
    public sealed class PointsCalculator : IPointsCalculator
    {
        public int CalculateMatchPoints(int predictedHome, int predictedAway, int actualHome, int actualAway, LeagueSettings s)
        {
            // Exakt resultat
            if (predictedHome == actualHome && predictedAway == actualAway)
                return s.PointsCorrectScore;

            var points = 0;

            // Rätt utkomst (W/D/L)
            if (Outcome(predictedHome, predictedAway) == Outcome(actualHome, actualAway))
                points += s.PointsCorrectOutcome;

            // Rätt mål per lag
            if (predictedHome == actualHome) points += s.PointsCorrectGoals;
            if (predictedAway == actualAway) points += s.PointsCorrectGoals;

            return points;
        }

        private static int Outcome(int h, int a) => h == a ? 0 : (h > a ? 1 : -1);
    }
}
