using System;
using System.Collections;
using Luxodd.Game.Scripts.Network;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerDataManager playerDataManager;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private GameObject gameComponents;

    private string _currentDifficulty;
    private int _currentScore;
    
    private float _gameTime;
    private bool _gameActive;
    private bool _canRestart;
    
    public static Action OnResetGame;
    public static Action OnContinueGame;
    public static Action<int> OnGameOver;
    public static Action<int, string> OnError;
    
    private int _errorCode;
    private string _errorMessage;

    private void Update()
    {
        if (_gameActive) _gameTime += Time.deltaTime;
    }
    
    private void OnEnable() 
    {
        Player.UpdateScore += UpdateScore;
        GameUI.GameStart += GameStart;
        GameUI.Exit += ExitWithoutErrors;
        GameUI.OnDifficultySelect += DifficultySelect;
        playerDataManager.DataSaveSuccess += ExitWithoutErrors;

        OnResetGame += ResetGame; 
        OnError += OnErrorInternal;
    }

    private void OnDisable()
    {
        Player.UpdateScore -= UpdateScore;
        GameUI.GameStart -= GameStart;
        GameUI.Exit -= ExitWithoutErrors;
        GameUI.OnDifficultySelect -= DifficultySelect;
        playerDataManager.DataSaveSuccess -= ExitWithoutErrors;
        
        OnResetGame -= ResetGame; 
        OnError -= OnErrorInternal;
    }
    
    private void GameStart()
    {
        LevelBegin();
        gameComponents.SetActive(true);
        _gameActive = true;
    }

    private void ResetGame()
    {
        _currentScore = 0;
        _gameTime = 0;
    }
    private void DifficultySelect(string difficulty) => _currentDifficulty = difficulty;
    public string GetCurrentDifficulty() => _currentDifficulty;

    private void LevelBegin()
    {
        _canRestart = false;
        networkManager.WebSocketCommandHandler.SendLevelBeginRequestCommand(0,OnLevelBeginSuccess, OnLevelBeginError);
    }

    private void OnLevelBeginSuccess()
    {
        Debug.LogWarning("Level Begin Success");
    }

    private void OnLevelBeginError(int code,string msg)
    {
        OnError?.Invoke(code, msg);
        _errorCode = code;
        _errorMessage = msg;
    }
    
    private void OnSessionOptionContinueSuccess(SessionOptionAction sessionOptionAction)
    {
        switch (sessionOptionAction)
        {
            case SessionOptionAction.Restart:
                Restart();
                break;
            case SessionOptionAction.Continue:
                OnContinueGame?.Invoke();
                _gameActive = true;
                break;
            case SessionOptionAction.End:
            case SessionOptionAction.Cancel:
                LevelEnd();
                playerDataManager.Save(_currentScore);
                break;
            default:
                Debug.LogError("SessionOptionAction not implemented");
                break;
        }
    }

    private void Restart()
    {
        _canRestart = true;
        LevelEnd();
        playerDataManager.UpdateScore(_currentScore);
        OnResetGame?.Invoke();
        _gameActive = true;
    }
    
    private void UpdateScore(int lengthOfSnake, Vector2 position)
    {
        _currentScore +=  lengthOfSnake*100;
    }

    public void GameOver()
    {
        OnGameOver?.Invoke(_currentScore);
        StartCoroutine(WaitAndShowPopup());
        
        playerDataManager.AddExperience(Mathf.RoundToInt(_gameTime));
        _gameActive = false;
    }
    private void LevelEnd()
    {
        networkManager.WebSocketCommandHandler.SendLevelEndRequestCommand(0, _currentScore, OnLevelEndSuccess, OnLevelEndError);
        Debug.LogWarning($"Level ended Sent, with score {_currentScore}");
    }

    private void OnLevelEndSuccess()
    {
        if (_canRestart) LevelBegin();
        Debug.LogWarning($"Level ended successfully");
    }

    private void OnLevelEndError(int code, string msg)
    {
        OnError?.Invoke(code, msg);
        _errorCode = code;
        _errorMessage = msg;
    }

    private void OnErrorInternal(int code, string msg)
    {
        _errorCode = code;
        _errorMessage = msg;
    }

    public void ExitWithoutErrors()
    {
        networkManager.WebSocketService.BackToSystem();
    }

    public void ExitWithErrors()
    {
        networkManager.WebSocketService.BackToSystemWithError(_errorMessage, _errorCode.ToString());
    }
    
    private IEnumerator WaitAndShowPopup()
    {
        yield return new WaitForSeconds(3f);
        networkManager.WebSocketService.SendSessionOptionContinue(OnSessionOptionContinueSuccess);
    }
}
