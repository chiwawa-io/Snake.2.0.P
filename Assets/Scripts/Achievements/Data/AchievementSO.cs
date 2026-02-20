using UnityEngine;

namespace Achievements.Data
{
    [CreateAssetMenu(fileName = "AchievementSO", menuName = "Snake/AchievementSO")]
    public class AchievementSO : ScriptableObject
    {
        public string id;
        public string displayName;
        public string description;
    }
}