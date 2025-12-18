using Domain.Entities;

namespace Application.Common.Interfaces
{
    public interface IPointsCalculator
    {
        int CalculateMatchPoints(int predictedHome, int predictedAway, int actualHome, int actualAway, LeagueSettings settings);
    }
}
