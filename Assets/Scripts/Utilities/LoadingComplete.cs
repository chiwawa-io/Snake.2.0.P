using System;
using UnityEngine;
using Game.Core;

public class LoadingComplete : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private PlayerDataManager playerDataManager;
    [SerializeField] private GameManager gameManager;

    public static Action LoadingCompleteAction;
 
    private void Start()
    {
        LoadingStart();
    }
    private void LoadingStart()
    {
        networkManager.WebSocketService.ConnectToServer(OnConnectionSuccess, OnConnectionFailure);
    }

    private void OnConnectionSuccess()
    {
        networkManager.HealthStatusCheckService.Activate();
        playerDataManager.LoadData();
        LoadingCompleteAction?.Invoke();
    }

    private void OnConnectionFailure()
    {
        gameManager.OnError(1, "Connection Failure");
    }

}
