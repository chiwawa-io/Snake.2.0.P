using UnityEngine;
using Game.Core; // Assuming GameManager is here
using Luxodd.Game.Scripts.Network;
using Zenject; // For Inactivity

namespace Game.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private MainMenuView mainMenuView;
        [SerializeField] private GameView hudView;
        [SerializeField] private ErrorView errorView;
        [SerializeField] private BaseView loadingView;
        [SerializeField] private BaseView leaderboardView;
        [SerializeField] private BaseView achievementsView;

        private PlayerDataManager _playerDataManager; 

        [Inject]
        public void Construct(PlayerDataManager playerDataManager)
        {
            _playerDataManager = playerDataManager;
        }

        private void OnEnable()
        {
            GameManager.OnStateChanged += HandleStateChanged;
            GameManager.OnErrorOccurred += HandleError;
            GameManager.OnScoreChanged += HandleScoreChange; 
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChanged;
            GameManager.OnErrorOccurred -= HandleError;
            GameManager.OnScoreChanged -= HandleScoreChange;
        }

        private void HandleStateChanged(GameState newState)
        {
            HideAllViews();

            switch (newState)
            {
                case GameState.Loading:
                    loadingView.Show();
                    break;

                case GameState.MainMenu:
                    mainMenuView.Show();
                    mainMenuView.UpdatePlayerStats(_playerDataManager.GetLevel(), _playerDataManager.GetExpNormalized());
                    break;

                case GameState.DifficultySelect:
                    mainMenuView.Show();
                    mainMenuView.SetDifficultyPopupActive(true);
                    break;

                case GameState.InGame:
                    hudView.Show();
                    hudView.UpdateScore(0); 
                    hudView.UpdateLives(3);
                    break;
                
                case GameState.Leaderboard:
                    leaderboardView.Show();
                    break;

                case GameState.Achievements:
                    achievementsView.Show();
                    break;

                case GameState.GameOver:
                    break;
            }
        }

        private void HandleError(int code, string message)
        {
            HideAllViews(); 
            errorView.SetErrorDetails(code.ToString(), message);
            errorView.Show();
        }

        private void HandleScoreChange(int newScore)
        {
            if (hudView.IsVisible)
            {
                hudView.UpdateScore(newScore);
            }
        }
        
        private void HideAllViews()
        {
            mainMenuView.Hide();
            hudView.Hide();
            errorView.Hide();
            loadingView.Hide();
            leaderboardView.Hide();
            achievementsView.Hide();
        }
    }
}