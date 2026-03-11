using System;
using Zenject;
using UnityEngine;
using System.Linq;
using Core.Events;
using Core.Enums;
using Gameplay.GameItems;
using Gameplay.Snake;
using Gameplay.Global.Data;
using Gameplay.Board;
using Services.Backend;
using Luxodd.Game.Scripts.Game;
using Luxodd.Game.Scripts.Missions;
using Cysharp.Threading.Tasks; 

namespace Gameplay.Global
{
    public class GameSessionController : IInitializable, IDisposable
    {
        private const int MinBoardWidth = 15;
        private const int MaxBoardWidth = 40;
        private const int DefaultBoardHeight = 22;
        private const float MaxHardness = 99f;

        private readonly SignalBus _signalBus;
        private readonly ItemSpawner _itemSpawner;
        private readonly SnakeModel _snakeModel;
        private readonly SnakeEngine _snakeEngine;
        private readonly GameObject _gameElements;
        private readonly BoardVisuals _boardVisuals;
        private readonly LevelBoardsConfig _levelBoundsConfig;
        private readonly LuxoddBackendService _backendService;
        
        private GameDifficulty _currentDifficulty = GameDifficulty.Medium;
        private GameType _currentGameType = GameType.Pay2Play;
        private Vector2Int bounds;

        public GameSessionController(
            SignalBus signalBus,
            ItemSpawner itemSpawner,
            SnakeModel snakeModel,
            SnakeEngine snakeEngine,
            [Inject(Id = "GameElements")] GameObject gameElements,
            LevelBoardsConfig levelBoardsConfig,
            BoardVisuals boardVisuals,
            LuxoddBackendService backendService)
        {
            _signalBus = signalBus;
            _itemSpawner = itemSpawner;
            _snakeModel = snakeModel;
            _snakeEngine = snakeEngine;
            _gameElements = gameElements;
            _boardVisuals = boardVisuals;
            _levelBoundsConfig = levelBoardsConfig;
            _backendService = backendService;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GameStateChangedSignal>(OnStateChanged);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<GameStateChangedSignal>(OnStateChanged);
        }

        public void SetDifficulty(GameDifficulty difficulty)
        {
            _currentDifficulty = difficulty;
        }

        private void OnStateChanged(GameStateChangedSignal signal)
        {
            if (signal.NewState == GameState.LevelLoading) 
            {
                StartNewSession();
            }
            else if (signal.NewState == GameState.MainMenu)
            {
                _itemSpawner.ResetSpawner();
                _gameElements.SetActive(false);
            }
        }

        private void StartNewSession()
        {
            _snakeEngine.Reset();

            _backendService.GetGameSessionInfo(
                onSuccess: (payload, sbData) => 
                {
                    if (payload.SessionType == "sb") _currentGameType = GameType.StrategicBetting;
                    else _currentGameType = GameType.Pay2Play;

                    if (_currentGameType == GameType.StrategicBetting)
                    {
                        InitializeStrategicBetting(sbData);
                        _gameElements.SetActive(true);
                        _signalBus.Fire(new GameStateChangedSignal(GameState.InGame));
                        
                        StartGameWithDelay(3.5f).Forget();
                    }
                    else
                    {
                        InitializeStandardGame();
                        _gameElements.SetActive(true);
                        _signalBus.Fire(new GameStateChangedSignal(GameState.InGame));
                    }
                },
                onError: (err, msg) => Debug.LogError(err + msg)
            );
        }

        private async UniTaskVoid StartGameWithDelay(float delaySeconds)
        {
            await UniTask.WaitForSeconds(delaySeconds);
            _signalBus.Fire(new GameStartedSignal());
        }

        private void InitializeStandardGame()
        {
            foreach(var boardConfig in _levelBoundsConfig.boardConfigs)
            {
                if (boardConfig.gameDifficulty == _currentDifficulty)
                    bounds = boardConfig.boardSize;
            }

            _itemSpawner.Initialize(bounds, _snakeModel.Body, _currentDifficulty, false);
            _snakeEngine.Initialize(bounds);
            _boardVisuals.GenerateBoard(bounds, _currentDifficulty);
        }

        private void InitializeStrategicBetting(StrategicBettingData payload)
        {
            var difficulty = CalculateGameHardness((int)payload.LevelDifficulty); 

            var primaryMission = payload.Missions.FirstOrDefault();
            int targetLength = primaryMission != null ? (int)primaryMission.Value : 20;
            
            var currentBounds = CalculateBounds((int)primaryMission.CalculatedHardness);

            _itemSpawner.Initialize(currentBounds, _snakeModel.Body, difficulty, true); 
            _snakeEngine.Initialize(currentBounds);
            _boardVisuals.GenerateBoard(currentBounds, difficulty);

            _signalBus.Fire(new StrategicBettingStartedSignal(targetLength, (int)payload.LevelDifficulty));
            _signalBus.Fire(new LifeUpdatedSignal(1));
            
            Debug.Log($"[SB] Started. Target Length: {targetLength}, Bounds: {currentBounds}");
        }

        private GameDifficulty CalculateGameHardness(int hardness)
        {
            return hardness switch
            {
                0 => GameDifficulty.Easy,
                1 => GameDifficulty.Medium,
                _ => GameDifficulty.Hard
            };
        }

        private Vector2Int CalculateBounds(int hardness)
        {
            float t = hardness / MaxHardness;
            int dynamicWidth = Mathf.RoundToInt(Mathf.Lerp(MaxBoardWidth, MinBoardWidth, t));
            return new Vector2Int(dynamicWidth, DefaultBoardHeight);             
        }
    }
}