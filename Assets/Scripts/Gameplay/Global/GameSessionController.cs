using System;
using Zenject;
using UnityEngine;
using System.Linq;
using Core.Events;
using Core.Enums;
using Cysharp.Threading.Tasks;
using Gameplay.GameItems;
using Gameplay.Snake;
using Gameplay.Global.Data;
using Gameplay.Board;
using Services.Backend;
using Luxodd.Game.Scripts.Game;
using Luxodd.Game.Scripts.Missions;
using SBetting;

namespace Gameplay.Global
{
    public class GameSessionController : IInitializable, IDisposable
    {
        private const float MissionBriefDelay = 3.5f; 
        private readonly SignalBus _signalBus;
        private readonly ItemSpawner _itemSpawner;
        private readonly SnakeModel _snakeModel;
        private readonly SnakeEngine _snakeEngine;
        private readonly GameObject _gameElements;
        private readonly BoardVisuals _boardVisuals;
        private readonly LevelBoardsConfig _levelBoundsConfig;
        private readonly LuxoddBackendService _backendService;
        private readonly HardnessEvaluator _hardnessEvaluator;

        private GameDifficulty _currentDifficulty = GameDifficulty.Medium;
        private GameType _currentGameType = GameType.Pay2Play;
        private Vector2Int _bounds;

        public GameSessionController(
            SignalBus signalBus,
            ItemSpawner itemSpawner,
            SnakeModel snakeModel,
            SnakeEngine snakeEngine,
            [Inject(Id = "GameElements")] GameObject gameElements,
            LevelBoardsConfig levelBoardsConfig,
            BoardVisuals boardVisuals,
            LuxoddBackendService backendService,
            HardnessEvaluator hardnessEvaluator)
        {
            _signalBus = signalBus;
            _itemSpawner = itemSpawner;
            _snakeModel = snakeModel;
            _snakeEngine = snakeEngine;
            _gameElements = gameElements;
            _boardVisuals = boardVisuals;
            _levelBoundsConfig = levelBoardsConfig;
            _backendService = backendService;
            _hardnessEvaluator = hardnessEvaluator;
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
                    _currentGameType = payload.SessionType == "sb"  
                        ? GameType.StrategicBetting 
                        : GameType.Pay2Play;

                    if (_currentGameType == GameType.StrategicBetting)
                    {
                        _gameElements.SetActive(true);
                        InitializeStrategicBetting(sbData);
                        _signalBus.Fire(new GameStateChangedSignal(GameState.InGame));
                        
                        StartGameWithDelay(MissionBriefDelay).Forget();
                    }
                    else
                    {
                        _gameElements.SetActive(true);
                        InitializeStandardGame();
                        _signalBus.Fire(new GameStateChangedSignal(GameState.InGame));
                        _signalBus.Fire(new GameStartedSignal());
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
                    _bounds = boardConfig.boardSize;
            }

            _itemSpawner.Initialize(_bounds, _snakeModel.Body, _currentDifficulty, false);
            _snakeEngine.Initialize(_bounds);
            _boardVisuals.GenerateBoard(_bounds, _currentDifficulty);
        }

        private void InitializeStrategicBetting(StrategicBettingData payload)
        {
            var difficulty = CalculateGameHardness((int)payload.LevelDifficulty); 

            var primaryMission = payload.Missions.FirstOrDefault();
            int targetLength = primaryMission != null ? primaryMission.Value : 20;
            
            var hardness = (int)primaryMission.CalculatedHardness;
            var spawnChance = 0; // temporary solution //TODO: PHASE 2 CALCULATION
            var currentBounds = _hardnessEvaluator.GetBoardSize(hardness);

            _itemSpawner.Initialize(currentBounds, _snakeModel.Body, difficulty, true); 
            _itemSpawner.SetSpawnChallengeChance(spawnChance);
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
    }
}