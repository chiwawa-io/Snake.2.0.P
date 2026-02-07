using System;
using UnityEngine;
using Zenject;

public enum GameState
{
    Loading,
    MainMenu,
    Leaderboard,
    Achievements,
    InGame,
    GameOver,
    Error,
    DifficultySelect,
    Save,
    Exit
}

namespace Game.Core 
{
    public class GameManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject gamePlayComponents;
        
        private NetworkManager _networkManager;
        private PlayerDataManager _playerDataManager;
        private InactivityDetector _inactivityDetector;

        public static event Action<GameState> OnStateChanged;
        public static event Action<int> OnScoreChanged;
        public static event Action OnGameStarted;
        public static event Action OnGameReset;
        public static event Action<int> OnGameOver;
        public static event Action<int, string> OnErrorOccurred;

        private GameState _currentState;
        public string CurrentDifficulty { get; private set; }

        public int CurrentScore { get; private set; }
        private bool _isFirstLoad = true;
        private bool _isPlaying;

        [Inject]
        public void Construct(
            NetworkManager networkManager, 
            PlayerDataManager playerDataManager, 
            InactivityDetector inactivityDetector)
        {
            _networkManager = networkManager;
            _playerDataManager = playerDataManager;
            _inactivityDetector = inactivityDetector;
        }

        private void OnEnable()
        {
            _playerDataManager.OnDataLoaded += HandleDataLoaded;
            InactivityDetector.TimesUp += HandleTimesUp;
            Player.UpdateScore += AddScore;
        }

        private void Start()
        {
            // HandleDataLoaded();
            
            SetState(GameState.Loading);
        }

        private void OnDestroy()
        {
            _playerDataManager.OnDataLoaded -= HandleDataLoaded;
            Player.UpdateScore -= AddScore;
            InactivityDetector.TimesUp -= HandleTimesUp;
        }

        private void HandleDataLoaded()
        {
            SetState(GameState.MainMenu);
        }

        public void RequestStartGame()
        {
            SetState(GameState.Loading);
            
            _networkManager.WebSocketCommandHandler.SendLevelBeginRequestCommand(
                0, 
                () => {
                    ResetGame();
                    StartGame();
                },
                (code, msg) => OnErrorOccurred?.Invoke(code, msg)
            );
        }

        public void StartGame()
        {
            _isPlaying = true;
            gamePlayComponents.SetActive(true);
            SetState(GameState.InGame);
            OnScoreChanged?.Invoke(CurrentScore);
            OnGameStarted?.Invoke();
        }

        public void ResetGame()
        {
            CurrentScore = 0;
            OnGameReset?.Invoke();
        }

        public void ReviveGame()
        {
            Time.timeScale = 1;
            SetState(GameState.InGame);
            OnGameReset?.Invoke();
            OnGameStarted?.Invoke();
        }

        public void AddScore(int amount, Vector2 position)
        {
            if (!_isPlaying) return;
            CurrentScore += amount;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void TriggerGameOver()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            
            Time.timeScale = 0;
            OnGameOver?.Invoke(CurrentScore); 
            SetState(GameState.GameOver);
        }

        public void SetState(GameState newState)
        {
            _inactivityDetector.StopDetector();
            _isFirstLoad = false;
            
            _currentState = newState;
            OnStateChanged?.Invoke(newState);
            
            _inactivityDetector.StartDetector();
        }

        public void SetDifficulty(string difficulty)
        {
            CurrentDifficulty = difficulty;
            RequestStartGame();
        }

        private void HandleTimesUp()
        {
            if (_isFirstLoad && _currentState == GameState.MainMenu)
            {
                StartGame(); 
                _isFirstLoad = false;
            }
            else if (!_isFirstLoad && _currentState != GameState.InGame)
            {
                _networkManager.WebSocketService.BackToSystem();
            }
        }

        public void OnError(int code, string msg)
        {
            OnErrorOccurred?.Invoke(code, msg);   
        }

        public void RequestExitWithError()
        {
            
        }
    }
}