using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.Buttons{
    public class ButtonManager : MonoBehaviour
    {
        [Header("Basic")]
        [SerializeField] private GameManager gameManager;
        
        [Header("Menu Buttons")]
        [SerializeField] private Button menuStartButton;
        [SerializeField] private Button menuLeaderboardButton;
        [SerializeField] private Button menuAchievementButton;
        [SerializeField] private Button menuExitButton;
        [Space]
        [SerializeField] private Button easyDifficultyButton;
        [SerializeField] private Button mediumDifficultyButton;
        [SerializeField] private Button hardDifficultyButton;
        [SerializeField] private Button returnDifficultyButton;
        
        [Header("Achievements Buttons")]
        [SerializeField] private Button achievementReturnButton;
        
        [Header("Leaderboard Buttons")]
        [SerializeField] private Button leaderboardReturnButton;
        
        [Header("Error Panel Buttons")]
        [SerializeField] private Button errorPanelExit;
        
        private void OnEnable()
        {
            menuStartButton.onClick.AddListener(() => gameManager.SetState(GameState.DifficultySelect));
            menuLeaderboardButton.onClick.AddListener(() => gameManager.SetState(GameState.Leaderboard));
            menuAchievementButton.onClick.AddListener(() => gameManager.SetState(GameState.Achievements));
            menuExitButton.onClick.AddListener(() => gameManager.SetState(GameState.Exit));
            easyDifficultyButton.onClick.AddListener(() => gameManager.SetDifficulty("Easy"));
            mediumDifficultyButton.onClick.AddListener(() => gameManager.SetDifficulty("Medium"));
            hardDifficultyButton.onClick.AddListener(() => gameManager.SetDifficulty("Hard"));
            returnDifficultyButton.onClick.AddListener(() => gameManager.SetState(GameState.MainMenu));
            achievementReturnButton.onClick.AddListener(() => gameManager.SetState(GameState.MainMenu));
            leaderboardReturnButton.onClick.AddListener(() => gameManager.SetState(GameState.MainMenu));
            errorPanelExit.onClick.AddListener(() => gameManager.RequestExitWithError());
        }

        private void OnDisable()
        {
            menuStartButton.onClick.RemoveListener(() => gameManager.SetState(GameState.DifficultySelect));
            menuLeaderboardButton.onClick.RemoveListener(() => gameManager.SetState(GameState.Leaderboard));
            menuAchievementButton.onClick.RemoveListener(() => gameManager.SetState(GameState.Achievements));
            menuExitButton.onClick.RemoveListener(() => gameManager.SetState(GameState.Exit));
            easyDifficultyButton.onClick.RemoveListener(() => gameManager.SetDifficulty("Easy"));
            mediumDifficultyButton.onClick.RemoveListener(() => gameManager.SetDifficulty("Medium"));
            hardDifficultyButton.onClick.RemoveListener(() => gameManager.SetDifficulty("Hard"));
            achievementReturnButton.onClick.RemoveListener(() => gameManager.SetState(GameState.MainMenu));
            leaderboardReturnButton.onClick.RemoveListener(() => gameManager.SetState(GameState.MainMenu));
            errorPanelExit.onClick.AddListener(() => gameManager.RequestExitWithError()); 
        }
    }
}
