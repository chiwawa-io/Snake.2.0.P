using System.Linq;
using Luxodd.Game.Scripts.Game.Leaderboard;
using UnityEngine;
using Game.UI;
using Zenject;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private LeaderboardView leaderboardView; 

    private NetworkManager _networkManager;

    [Inject]
    public void Construct(NetworkManager networkManager)
    {
        _networkManager = networkManager;
    }

    public void OnEnable()
    {
        FetchLeaderboard();
    }

    public void FetchLeaderboard()
    {
        _networkManager.WebSocketCommandHandler.SendLeaderboardRequestCommand(
            OnSuccess, 
            (code, msg) => Debug.LogError($"Leaderboard Error: {msg}")
        );
    }

    private void OnSuccess(LeaderboardDataResponse response)
    {
        if (response.Leaderboard == null) return;

        var list = response.Leaderboard.ToList();
        
        leaderboardView.Populate(list);
    }
}