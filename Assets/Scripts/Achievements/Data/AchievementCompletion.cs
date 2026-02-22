using System.Collections.Generic;
using UnityEngine;

namespace Achievements.Data
{

    [CreateAssetMenu(fileName = "SnakeAchievementConfig", menuName = "Snake/Achievement Completion")]
    public class AchievementCompletion : ScriptableObject
    {
        [System.Serializable]
        public struct AchievementRule
        {
            public int Threshold;
            public string AchievementId;
            public string AchievementName;
        }

        [Header("Collection Achievements")] public List<AchievementRule> FoodRules;
        public List<AchievementRule> SpeedPowerUpRules;

        [Header("State Achievements")] public List<AchievementRule> SnakeSizeRules;
    }
}