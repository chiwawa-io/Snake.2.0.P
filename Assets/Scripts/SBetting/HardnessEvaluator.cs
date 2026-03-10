using UnityEngine;

namespace SBetting
{
    public class HardnessEvaluator
    {
        // Constants define our "Safe Ranges"
        private const float MinGrowthTime = 4.0f;  // Hardness 99
        private const float MaxGrowthTime = 15.0f; // Hardness 1
        
        private const float MinChallengeChance = 0.1f; // Hardness 1
        private const float MaxChallengeChance = 0.7f; // Hardness 99

        private const int MinWidth = 15;
        private const int MaxWidth = 40;

        // Public API
        public float GetGrowthTimerDuration(int hardness)
        {
            float t = (hardness - 1) / 98f; // Normalize 1-99 to 0-1
            return Mathf.Lerp(MaxGrowthTime, MinGrowthTime, t);
        }

        public float GetTrapSpawnChance(int hardness)
        {
            float t = (hardness - 1) / 98f;
            return Mathf.Lerp(MinChallengeChance, MaxChallengeChance, t);
        }

        public Vector2Int GetBoardSize(int hardness)
        {
            float t = (hardness - 1) / 98f;
            int width = Mathf.RoundToInt(Mathf.Lerp(MaxWidth, MinWidth, t));
            return new Vector2Int(width, 22); // Height stays constant per your current code
        }
    }
}