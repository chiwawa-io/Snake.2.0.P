using System;
using Core.Enums;
using Core.Events;
using Gameplay.GameItem;
using Gameplay.Snake;
using Zenject;
using UnityEngine;
using Gameplay.Global.Data;
using Gameplay.Board;

namespace Gameplay.Global
{
    public class GameSessionController : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly ItemSpawner _itemSpawner;
        private readonly SnakeModel _snakeModel;
        private readonly SnakeEngine _snakeEngine;
        private readonly GameObject _gameElements;
        private readonly BoardVisuals _boardVisuals;
        private readonly LevelBoardsConfig _levelBoundsConfig;
        private GameDifficulty _currentDifficulty = GameDifficulty.Medium;
        private Vector2Int bounds;

        public GameSessionController(
            SignalBus signalBus,
            ItemSpawner itemSpawner,
            SnakeModel snakeModel,
            SnakeEngine snakeEngine,
            [Inject(Id = "GameElements")] GameObject gameElements,
            LevelBoardsConfig levelBoardsConfig,
            BoardVisuals boardVisuals)
        {
            _signalBus = signalBus;
            _itemSpawner = itemSpawner;
            _snakeModel = snakeModel;
            _snakeEngine = snakeEngine;
            _gameElements = gameElements;
            _boardVisuals = boardVisuals;
            _levelBoundsConfig = levelBoardsConfig;
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
            if (signal.NewState == GameState.InGame)
            {
                StartNewSession();
                _signalBus.Fire(new GameStartedSignal());
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
            _gameElements.SetActive(true);

            
            //TODO: Get mission data here

            foreach(var boardConfig in _levelBoundsConfig.boardConfigs)
            {
                if (boardConfig.gameDifficulty == _currentDifficulty)
                    bounds = boardConfig.boardSize;
            }

            _itemSpawner.Initialize(bounds, _snakeModel.Body, _currentDifficulty);
            _snakeEngine.Initialize(bounds);
            _boardVisuals.SetBoard(_currentDifficulty);
        }
    }
}