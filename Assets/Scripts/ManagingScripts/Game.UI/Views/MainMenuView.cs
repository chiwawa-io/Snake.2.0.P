using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class MainMenuView : BaseView
    {
        [Header("Player Info")]
        [SerializeField] private Slider expBar;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Difficulty Popup")]
        [SerializeField] private GameObject difficultyPopupRoot;
        [SerializeField] private Button difficultyFirstButton;
        [SerializeField] private Button difficultyCloseButton; // Optional, if you have a back button inside

        public void UpdatePlayerStats(int level, float normalizedExp)
        {
            if (levelText) levelText.text = level.ToString();
            if (expBar) expBar.value = normalizedExp;
        }

        public void SetDifficultyPopupActive(bool isActive)
        {
            if (difficultyPopupRoot) 
                difficultyPopupRoot.SetActive(isActive);

            if (isActive && difficultyFirstButton != null)
            {
                difficultyFirstButton.Select();
            }
            else if (!isActive)
            {
                base.Show(); 
            }
        }
    }
}