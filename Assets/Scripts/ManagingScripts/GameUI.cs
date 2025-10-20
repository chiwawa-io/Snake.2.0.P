using UnityEngine;
using System.Collections.Generic;

namespace Game.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Screen Controllers")]
        [SerializeField] private BaseUIController loadingScreen;
        [SerializeField] private BaseUIController mainMenuScreen;
        [SerializeField] private BaseUIController gameHUDScreen;
        [SerializeField] private BaseUIController gameOverScreen;
        [SerializeField] private BaseUIController leaderboardScreen;
        [SerializeField] private BaseUIController achievementsScreen;

        [Header("Panel Controllers")]
        [SerializeField] private ErrorUIController errorPanel;
        [SerializeField] private BaseUIController savingPanel;

        private List<BaseUIController> _allScreens;

        private void Awake()
        {
            _allScreens = new List<BaseUIController>
            {
                loadingScreen, mainMenuScreen, gameHUDScreen, gameOverScreen, 
                leaderboardScreen, achievementsScreen
            };
        }

        private void OnEnable()
        {
            // Subscribe to high-level game state events
            GameManager.OnGameOver += ShowGameOverScreen;
            GameManager.OnError += ShowError;
            GameManager.OnSaveStart += ShowSavingPanel;
            LoadingComplete.LoadingCompleteAction += ShowMainMenuScreen;
        }

        private void OnDisable()
        {
            GameManager.OnGameOver -= ShowGameOverScreen;
            GameManager.OnError -= ShowError;
            GameManager.OnSaveStart -= ShowSavingPanel;
            LoadingComplete.LoadingCompleteAction -= ShowMainMenuScreen;
        }

        private void ShowMainMenuScreen()
        {
            SetActiveScreen(mainMenuScreen);
        }

        private void ShowGameOverScreen(int finalScore)
        {
            SetActiveScreen(gameOverScreen);
        }

        private void ShowError(int code, string message)
        {
            errorPanel.ShowError(code, message);
        }

        private void ShowSavingPanel()
        {
            savingPanel.Show();
        }

        private void SetActiveScreen(BaseUIController activeScreen)
        {
            foreach (var screen in _allScreens)
            {
                if (screen != activeScreen && screen.IsVisible)
                {
                    screen.Hide();
                }
            }
            
            if (activeScreen != null && !activeScreen.IsVisible)
            {
                activeScreen.Show();
            }
        }
    }
}