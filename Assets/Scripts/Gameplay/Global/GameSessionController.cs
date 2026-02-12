// Scripts/Core/Controllers/GameSessionController.cs

using System;
using Core.Enums;
using Core.Events;
using Gameplay.Snake;
using Zenject;
using UnityEngine;

public class GameSessionController : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;
    private readonly ItemSpawner _itemSpawner;
    private readonly SnakeModel _snakeModel;
    private readonly SnakeEngine _snakeEngine;
    private readonly SnakeGameController _gameController; // To start/stop tick

    private string _currentDifficulty = "Medium"; // Default

    public GameSessionController(
        SignalBus signalBus, 
        ItemSpawner itemSpawner, 
        SnakeModel snakeModel,
        SnakeEngine snakeEngine,
        SnakeGameController gameController)
    {
        _signalBus = signalBus;
        _itemSpawner = itemSpawner;
        _snakeModel = snakeModel;
        _snakeEngine = snakeEngine;
        _gameController = gameController;
    }

    public void Initialize()
    {
        _signalBus.Subscribe<GameStateChangedSignal>(OnStateChanged);
    }

    public void Dispose()
    {
        _signalBus.Unsubscribe<GameStateChangedSignal>(OnStateChanged);
    }

    public void SetDifficulty(string difficulty)
    {
        _currentDifficulty = difficulty;
    }

    private void OnStateChanged(GameStateChangedSignal signal)
    {
        if (signal.NewState == GameState.InGame)
        {
            StartNewSession();
        }
        else if (signal.NewState == GameState.MainMenu)
        {
            _itemSpawner.ResetSpawner();
            // _snakeEngine.Reset(); // Done in GameController usually, but can be here
        }
    }

    private void StartNewSession()
    {
        // 1. Reset Logic
        _snakeEngine.Reset();
        
        // 2. Initialize Spawner
        // We pass the Model's Body list directly so Spawner can check for collisions
        // Note: You need to update ItemSpawner.Initialize signature slightly to accept this cleanly
        Vector2Int bounds = new Vector2Int(10, 10); // Or inject Grid Size
        _itemSpawner.Initialize(bounds, _snakeModel.Body, _currentDifficulty);
    }
}