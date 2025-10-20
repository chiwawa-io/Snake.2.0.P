using System;
using System.Collections;
using UnityEngine;

public class LoadingComplete : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private PlayerDataManager playerDataManager;

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
    }

    private void OnConnectionFailure()
    {
        GameManager.OnError?.Invoke(1, "Connection Failure");
    }

}
