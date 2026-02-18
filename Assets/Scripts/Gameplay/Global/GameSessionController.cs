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
    private readonly GameObject _gameElements;
    private string _currentDifficulty = "Medium";

    public GameSessionController(
        SignalBus signalBus, 
        ItemSpawner itemSpawner, 
        SnakeModel snakeModel,
        SnakeEngine snakeEngine,
        [Inject(Id = "GameElements")] GameObject gameElements)
    {
        _signalBus = signalBus;
        _itemSpawner = itemSpawner;
        _snakeModel = snakeModel;
        _snakeEngine = snakeEngine;
        _gameElements = gameElements;
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
        
        Vector2Int bounds = new Vector2Int(22, 22);
        _itemSpawner.Initialize(bounds, _snakeModel.Body, _currentDifficulty);
    }
}