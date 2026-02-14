using TMPro;
using UI.MainMenu.Presenters;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.MainMenu.Views
{
    public class MainMenuView : BaseView
    {
        [Header("Player Info")]
        [SerializeField] private Slider expBar;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Main Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button achievementsButton; 
        [SerializeField] private Button quitButton;

        [Header("Difficulty Popup")]
        [SerializeField] private GameObject difficultyPopupRoot;
        [SerializeField] private Button diffEasyButton;
        [SerializeField] private Button diffMediumButton;
        [SerializeField] private Button diffHardButton;
        [SerializeField] private Button diffCloseButton;

        [Inject] private MainMenuPresenter _presenter;

        private void Start()
        {
            startButton.onClick.AddListener(() => _presenter.OnPlayClicked());
            
            if(leaderboardButton) 
                leaderboardButton.onClick.AddListener(() => _presenter.OnLeaderboardClicked());
            
            if(achievementsButton)
                achievementsButton.onClick.AddListener(() => _presenter.OnAchievementsClicked());

            if(quitButton) 
                quitButton.onClick.AddListener(() => _presenter.OnQuitClicked());

            diffEasyButton.onClick.AddListener(() => _presenter.OnDifficultySelected("Easy"));
            diffMediumButton.onClick.AddListener(() => _presenter.OnDifficultySelected("Medium"));
            diffHardButton.onClick.AddListener(() => _presenter.OnDifficultySelected("Hard"));

            diffCloseButton.onClick.AddListener(() => _presenter.OnDifficultyClosed());
        }

        public override void Show()
        {
            base.Show();
            _presenter.OnShow();
        }

        public void UpdatePlayerStats(int level, float normalizedExp)
        {
            if (levelText) levelText.text = level.ToString();
            if (expBar) expBar.value = normalizedExp;
        }

        public void SetDifficultyPopupActive(bool isActive)
        {
            if (difficultyPopupRoot) 
                difficultyPopupRoot.SetActive(isActive);

            if (isActive)
            {
                // Auto-select "Easy" or the first button for controller support
                if(diffEasyButton) diffEasyButton.Select();
            }
            else
            {
                // Return focus to Start button when popup closes
                if(startButton) startButton.Select();
            }
        }
    }
}