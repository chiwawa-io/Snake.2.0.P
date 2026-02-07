using System;
using UnityEngine;
using Game.Core;
using Zenject;

public class LoadingComplete : MonoBehaviour
{
    private NetworkManager _networkManager;
    private PlayerDataManager _playerDataManager;
    private GameManager _gameManager;

    public static Action LoadingCompleteAction;

    [Inject]
    public void Construct(NetworkManager networkManager, PlayerDataManager playerDataManager, GameManager gameManager)
    {
        _networkManager = networkManager;
        _playerDataManager = playerDataManager;
        _gameManager = gameManager;
    }

    private void Start()
    {
        LoadingStart();
        // LoadingCompleteAction?.Invoke();
    }
    private void LoadingStart()
    {
        _networkManager.WebSocketService.ConnectToServer(OnConnectionSuccess, OnConnectionFailure);
    }

    private void OnConnectionSuccess()
    {
        _networkManager.HealthStatusCheckService.Activate();
        _playerDataManager.LoadData();
        LoadingCompleteAction?.Invoke();
    }

    private void OnConnectionFailure()
    {
        _gameManager.OnError(1, "Connection Failure");
    }

}
