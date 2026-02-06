using UnityEngine;
using TMPro;

namespace Game.UI
{
    public class LeaderboardRow : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI scoreText;

        public void SetData(int rank, string playerName, long score)
        {
            if (rankText) rankText.text = rank.ToString();

            if (nameText) nameText.text = string.IsNullOrEmpty(playerName) ? "Unknown" : playerName;

            if (scoreText) scoreText.text = score.ToString("D10");
        }

        public void SetEmpty()
        {
            if (rankText) rankText.text = "-";
            if (nameText) nameText.text = "---";
            if (scoreText) scoreText.text = "0000000000";
        }
    }
}