using System;
using System.Collections;
using UnityEngine;

public class LoadingComplete : MonoBehaviour
{
    [SerializeField] private int waitTime;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private PlayerDataManager playerDataManager;

    public static Action LoadingCompleteAction;
 
    private void Start()
    {
        StartCoroutine(LoadingRoutine());
    }

    private void OnConnectionSuccess()
    {
        networkManager.HealthStatusCheckService.Activate();
        playerDataManager.LoadData();
    }
    private void OnConnectionFailure() {GameManager.OnError?.Invoke(1, "Connection Failure"); }

    private IEnumerator LoadingRoutine()
    {
        networkManager.WebSocketService.ConnectToServer(OnConnectionSuccess, OnConnectionFailure);
        
        yield return new WaitForSeconds(waitTime);
        
        LoadingCompleteAction?.Invoke();
    }
}
