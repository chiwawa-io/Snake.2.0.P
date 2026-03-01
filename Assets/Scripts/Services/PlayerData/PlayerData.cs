using System.Collections.Generic;
using Newtonsoft.Json;

namespace Services.PlayerData
{
    [System.Serializable]
    public class PlayerData
    {
        public int BestScore;
        public int Level;

        public int Xp;
        
        [JsonProperty("completed_achievements")]
        public HashSet<string> CompletedAchievementIds { get; set; } = new();

        public PlayerData(int score, int level, int xp,
            HashSet<string> completedAchievementIds)
        {
            BestScore = score;
            Level = level;
            Xp = xp;
            CompletedAchievementIds = completedAchievementIds ?? new();
        }

        public PlayerData()
        {
        }
    }
}
