using TMPro;
using UnityEngine;

namespace Achievements
{
    public class AchievementsRowUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _achievementName;
        [SerializeField] private TextMeshProUGUI _achievementDescription;
        [SerializeField] private TextMeshProUGUI _achievementIsCompleted;

        public void Setup(string displayName, bool isCompleted, string description)
        {
            _achievementName.text = displayName;
            _achievementIsCompleted.text = isCompleted ? "Completed" : "Not Completed";
            _achievementDescription.text = description;
        }
    }
}