using TMPro;
using UnityEngine;

public class AchievementsRowUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI achievementName;
    [SerializeField] private TextMeshProUGUI achievementDescription;
    [SerializeField] private TextMeshProUGUI achievementIsCompleted;
    public void Setup(string displayName, bool isCompleted, string description)
    {
        achievementName.text = displayName;
        achievementIsCompleted.text = isCompleted ? "Completed" : "Not Completed";
        achievementDescription.text = description;
    }
}
