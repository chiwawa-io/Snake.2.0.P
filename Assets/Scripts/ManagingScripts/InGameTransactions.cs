using UnityEngine;
using Luxodd.Game.Scripts.Network;

public class InGameTransactions : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private PlayerDataManager playerDataManager;
    
    private void OnEnable()
    {
        GameManager.OnGameOverSequenceCompleted += ShowSessionOptions;
    }

    private void OnDisable()
    {
        GameManager.OnGameOverSequenceCompleted -= ShowSessionOptions;
    }

    private void ShowSessionOptions()
    {
        networkManager.WebSocketService.SendSessionOptionContinue(OnSessionOptionContinueSuccess);
    }

    private void OnSessionOptionContinueSuccess(SessionOptionAction sessionOptionAction)
    {
        switch (sessionOptionAction)
        {
            case SessionOptionAction.Restart:
                RestartGame();
                break;
            case SessionOptionAction.Continue:
                gameManager.ContinueGame();
                break;
            case SessionOptionAction.End:
            case SessionOptionAction.Cancel:
                EndGameSession();
                break;
            default:
                Debug.LogError("SessionOptionAction not implemented");
                break;
        }
    }

    private void RestartGame()
    {
        gameManager.PrepareForRestart();
        playerDataManager.UpdateScore(gameManager.GetCurrentScore());
        GameManager.OnResetGame?.Invoke();
        gameManager.StartNewGame();
    }

    private void EndGameSession()
    {
        gameManager.PrepareForEnd();
        playerDataManager.Save(gameManager.GetCurrentScore());
        GameManager.OnSaveStart?.Invoke();
    }
}
