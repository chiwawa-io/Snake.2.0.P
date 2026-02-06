using UnityEngine;
using Luxodd.Game.Scripts.Network;
using Game.Core;
using Zenject;

public class InGameTransactions : MonoBehaviour
{
    private GameManager _gameManager;
    private NetworkManager _networkManager;
    private PlayerDataManager _playerDataManager;
    
    [Inject]
    public void Construct(GameManager gameManager, NetworkManager networkManager, PlayerDataManager playerDataManager)
    {
        _gameManager = gameManager;
        _networkManager = networkManager;
        _playerDataManager = playerDataManager;
    }

    private void OnEnable()
    {
        GameManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= HandleGameOver;
    }

    private void HandleGameOver(int finalScore)
    {
        _networkManager.WebSocketService.SendSessionOptionContinue(OnContinueDecision);
    }

    private void OnContinueDecision(SessionOptionAction action)
    {
        switch (action)
        {
            case SessionOptionAction.Continue:
                _gameManager.ReviveGame(); 
                break;

            case SessionOptionAction.End:
                FinalizeAndOfferRestart();
                break;
                
            case SessionOptionAction.Restart: 
                 FinalizeAndOfferRestart(true);
                 break;
        }
    }

    private void FinalizeAndOfferRestart(bool autoTriggerRestart = false)
    {
        int score = _gameManager.CurrentScore;
        
        _networkManager.WebSocketCommandHandler.SendLevelEndRequestCommand(
            0, 
            score, 
            () => {
                _playerDataManager.SaveGameSession(score);

                if(autoTriggerRestart) 
                {
                     PerformRestart();
                }
                else 
                {
                     _networkManager.WebSocketService.SendSessionOptionRestart(OnRestartDecision);
                }
            }, 
            (code, msg) => _gameManager.OnError(code, msg)
        );
    }

    private void OnRestartDecision(SessionOptionAction action)
    {
        if (action == SessionOptionAction.Restart)
        {
            PerformRestart();
        }
        else
        {
            _networkManager.WebSocketService.BackToSystem();
        }
    }

    private void PerformRestart()
    {
        _gameManager.ResetGame();
        
        _networkManager.WebSocketCommandHandler.SendLevelBeginRequestCommand(
            0, 
            () => _gameManager.StartGame(),
            (code, msg) => _gameManager.OnError(code, msg)
        );
    }
}