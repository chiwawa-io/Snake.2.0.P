using System;
using System.Collections;
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
    public static Action OnSaveStart;
    public static Action<int, string> OnError;

    public static Action OnGameOverSequenceCompleted;
    public static Action<string> StateChange;
    
    private int _errorCode;
    private string _errorMessage;

    private void Update()
    {
        if (_gameActive) _gameTime += Time.deltaTime;
    }
    
    private void OnEnable() 
    {
        Player.UpdateScore += UpdateScore;
        // GameUI.GameStart += GameStart;
        // GameUI.Exit += ExitWithoutErrors;
        // GameUI.OnDifficultySelect += DifficultySelect;
        playerDataManager.DataSaveSuccess += ExitWithoutErrors;

        OnResetGame += ResetGame; 
        OnError += OnErrorInternal;
    }

    private void OnDisable()
    {
        Player.UpdateScore -= UpdateScore;
        // GameUI.GameStart -= GameStart;
        // GameUI.Exit -= ExitWithoutErrors;
        // GameUI.OnDifficultySelect -= DifficultySelect;
        playerDataManager.DataSaveSuccess -= ExitWithoutErrors;
        
        OnResetGame -= ResetGame; 
        OnError -= OnErrorInternal;
    }
    
    private void GameStart()
    {
        StartNewGame();
    }
    
    public void StartNewGame()
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
    public int GetCurrentScore() => _currentScore;


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

    public void ContinueGame()
    {
        OnContinueGame?.Invoke();
        _gameActive = true;
    }

    public void PrepareForRestart()
    {
        _canRestart = true;
        LevelEnd();
    }
    
    public void PrepareForEnd()
    {
        _canRestart = false;
        LevelEnd();
    }
    
    private void UpdateScore(int lengthOfSnake, Vector2 position)
    {
        _currentScore +=  lengthOfSnake*100;
    }

    public void GameOver()
    {
        playerDataManager.AddExperience(Mathf.RoundToInt(_gameTime));
        _gameActive = false;
        StartCoroutine(WaitAndShowPopup());
        OnGameOver?.Invoke(_currentScore);
    }

    private void LevelEnd()
    {
        networkManager.WebSocketCommandHandler.SendLevelEndRequestCommand(0, _currentScore, OnLevelEndSuccess, OnLevelEndError);
        Debug.LogWarning($"Level ended Sent, with score {_currentScore}");
    }

    private void OnLevelEndSuccess()
    {
        if (_canRestart)
        {
        }
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
        OnGameOverSequenceCompleted?.Invoke();
    }
}