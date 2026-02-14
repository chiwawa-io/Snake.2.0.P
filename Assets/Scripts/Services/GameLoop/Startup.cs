using Core.Enums;
using Core.Events;
using Services.Backend;
using Services.PlayerData; 
using UnityEngine;
using Zenject;

public class Startup : IInitializable
{
    private readonly SignalBus _signalBus;
    private readonly IBackendService _backendService;
    private readonly PlayerDataManager _playerDataManager; 

    public Startup(
        SignalBus signalBus, 
        IBackendService backendService,
        PlayerDataManager playerDataManager)
    {
        _signalBus = signalBus;
        _backendService = backendService;
        _playerDataManager = playerDataManager;
    }

    public void Initialize()
    {
        _signalBus.Fire(new GameStateChangedSignal { NewState = GameState.Loading });

        _backendService.Initialize(OnConnectionSuccess, OnConnectionError);
    }

    private void OnConnectionSuccess()
    {
        Debug.Log("Luxodd Connected!");

        _playerDataManager.LoadData(); 

        Debug.Log("Transitioning to Menu...");
        _signalBus.Fire(new GameStateChangedSignal { NewState = GameState.MainMenu });
    }

    private void OnConnectionError()
    {
        var error = "ConnectionFailed";
        Debug.LogWarning(error);
        _signalBus.Fire(new ErrorSignal { Code = 500, Message = error });
    }
}