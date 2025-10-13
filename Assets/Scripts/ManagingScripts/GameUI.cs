using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("States")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private GameObject leaderboardScreen;
    [SerializeField] private GameObject achievementScreen;

    [Header("Menu")] 
    [SerializeField] private Slider expBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject difficultyPopup;
    [SerializeField] private TextMeshProUGUI menuInactivityText;
     
    [Header("Game Time")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI addedScoreText;
    [SerializeField] private List<GameObject> lifeUI;
    [SerializeField] private GameObject achievementUIInGame;
    [SerializeField] private TextMeshProUGUI achievementNameText;

    [Header("Game Panels")]
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI yourScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [Space] 
    [SerializeField] private GameObject gameSavingPanel;
    
    [Header("Error Panel")]
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private TextMeshProUGUI errorCodeText;
    [SerializeField] private TextMeshProUGUI errorMessageText;
    [SerializeField] private GameObject errorPanelButton;
    
    [Header ("Scripts")]
    [SerializeField] private PlayerDataManager playerDataManager;
    
    [SerializeField] private InactivityDetector timerScript;
    
    [Header("Timers")]
    [SerializeField] private TextMeshProUGUI timerInMenu; 
    [SerializeField] private TextMeshProUGUI timerInLeaderboard; 
    [SerializeField] private TextMeshProUGUI timerInAchievement;
    
    [Header("Buttons")]
    [SerializeField] private Button menuButton;
    [SerializeField] private Button difficultyButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button achievementButton;

    private int _score;
    
    private bool _firstTimeOnMenu = true; 

    
    private enum GameState
    {
        Loading,
        MainMenu,
        Game,
        Leaderboard,
        Achievements
    }

    private GameState _gameState;

    public static Action<string> OnDifficultySelect;
    public static Action GameStart;
    public static Action Exit;
    
    private void OnEnable()
    {
        GameManager.OnGameOver += GameOver;
        GameManager.OnError += OnError;
        GameManager.OnResetGame += ResetGameUI;
        GameManager.OnContinueGame += ResetGameUIContinue;
        Player.UpdateScore += AddedScoreNumber;
        Player.EventMessages += PrintMessage;
        Player.UpdateLife += UpdateLife;
        InactivityDetector.OnTimerUpdate += UpdateTimer;
        InactivityDetector.TimesUp += TimesUp; 
        LoadingComplete.LoadingCompleteAction += LoadMainMenu;
        AchievGameListener.OnAchievementCompleted += ShowAchievementNotif;
        
        playerDataManager.DataSaveStart += SaveDataUIShow;
    }
    
    private void OnDisable()
    {
        GameManager.OnGameOver -= GameOver;
        GameManager.OnError -= OnError;
        GameManager.OnResetGame -= ResetGameUI;
        GameManager.OnContinueGame -= ResetGameUIContinue;
        Player.UpdateScore -= AddedScoreNumber;
        Player.EventMessages -= PrintMessage;
        Player.UpdateLife -= UpdateLife;
        InactivityDetector.OnTimerUpdate -= UpdateTimer;
        InactivityDetector.TimesUp -= TimesUp;
        LoadingComplete.LoadingCompleteAction -= LoadMainMenu;
        AchievGameListener.OnAchievementCompleted -= ShowAchievementNotif;
        
        playerDataManager.DataSaveStart -= SaveDataUIShow;
    }

    public void StateSwitchButtonClick(string action)
    {
        
        switch (action)
        {
           case "Loading":
               TurnOffEveryScreen();
               loadingScreen.SetActive(true);
               _gameState = GameState.Loading;
               break;
           case "MainMenu":
               LoadMainMenu();
               break;
           case "GameE":
               LoadGame();
               break;
           case "GameM":
               LoadGame("Medium");
               break;
           case "GameH":
               LoadGame("Hard");
               break;
           case "OpenPopUp":
               ShowDifficultyPopup(true);
               break;
           case "ClosePopUp":
               ShowDifficultyPopup(false);
               break;
           case "Leaderboard":
               TurnOffEveryScreen();
               leaderboardScreen.SetActive(true);
               leaderboardButton.Select();
               _gameState = GameState.Leaderboard;
               timerScript.StartDetector();
               break;
           case "Achievements":
               TurnOffEveryScreen();
               achievementScreen.SetActive(true);
               achievementButton.Select();
               _gameState = GameState.Achievements;
               timerScript.StartDetector();
               break;
           default:
               TurnOffEveryScreen();
               mainMenuScreen.SetActive(true);
               _gameState = GameState.MainMenu;
               timerScript.StartDetector();
               break;
        }
    }


    private void TimesUp()
    {
        switch (_gameState)
        {
            case GameState.Leaderboard:
                Exit?.Invoke();
                break;
            case GameState.MainMenu:
                LoadGame();
                break;
        }

    }
    
    private void LoadMainMenu()
    {
        if (!_firstTimeOnMenu) Exit?.Invoke();
        
        TurnOffEveryScreen();
        difficultyPopup.SetActive(false);
        timerScript.StartDetector();

        expBar.value = playerDataManager.GetExp();
        levelText.text = playerDataManager.GetLevel().ToString();
        mainMenuScreen.SetActive(true);
        menuButton.Select();
        _gameState = GameState.MainMenu;
    }

    private void ShowDifficultyPopup(bool show)
    {
        difficultyPopup.SetActive(show);
        
        if (show) difficultyButton.Select(); 
        else menuButton.Select();
    }
    
    private void LoadGame(string difficulty = "Easy")
    {
        _firstTimeOnMenu = false;
        menuInactivityText.text = "Exit to Menu in: ";
        
        OnDifficultySelect?.Invoke(difficulty);
        GameManager.OnResetGame?.Invoke();
        
        TurnOffEveryScreen();
        GameStart?.Invoke();
        timerScript.StopDetector();
        
        gameScreen.SetActive(true);
        _gameState = GameState.Game;
    }

    private void ResetGameUI()
    {
        _score = 0;
        UpdateScoreText(0);
        UpdateLife(3);
        gameOverPanel.SetActive(false);
    }

    private void ResetGameUIContinue()
    {
        UpdateLife(3);
        gameOverPanel.SetActive(false);
    }

    private void UpdateScoreText(int score)
    {
        _score += score;
        scoreText.text = _score.ToString("D10");
    }

    private void UpdateLife(int lives)
    {
        lives = Mathf.Max(0, lives);

        for (var i = 0; i < lifeUI.Count; i++)
        {
            if (i < lives)
            {
                lifeUI[i].SetActive(true);
            }
            else
            {
                lifeUI[i].SetActive(false);
            }
        }
    }
    
    private void AddedScoreNumber(int bodyLength, Vector2 position)
    {
        UpdateScoreText(bodyLength*100);
        
        addedScoreText.gameObject.SetActive(true);
        addedScoreText.gameObject.transform.position = position;
        
        if (bodyLength>0)
            addedScoreText.text = "+" + (bodyLength*100);
        else
            addedScoreText.text = "" + (bodyLength*100);
        StartCoroutine(WaitAndTurnOff(addedScoreText.gameObject));
    }
    private void PrintMessage(string message, Vector2 position)
    {
        addedScoreText.gameObject.SetActive(true);
        addedScoreText.gameObject.transform.position = position;
        addedScoreText.text = message;
        StartCoroutine(WaitAndTurnOff(addedScoreText.gameObject));
    }
    
    private void GameOver(int score)
    {
        gameOverText.SetActive(true);
        StartCoroutine(WaitAndTurnOff(gameOverText, 1.5f, true, score));
        
    }

    private void OnError(int code, string message)
    {
        errorPanel.SetActive(true);
        errorPanelButton.GetComponent<Button>().Select();
        errorCodeText.text = code.ToString();
        errorMessageText.text = message;
    }

    private void UpdateTimer(int timeLeft)
    {
      switch (_gameState)
      {
          case GameState.Leaderboard:
              timerInLeaderboard.text = timeLeft.ToString();
              break;
          case GameState.MainMenu:
              timerInMenu.text = timeLeft.ToString();
              break;
          case GameState.Achievements:
              timerInAchievement.text = timeLeft.ToString();
              break;
      }
    }

    private void TurnOffEveryScreen()
    {
        loadingScreen.SetActive(false);
        gameScreen.SetActive(false);
        mainMenuScreen.SetActive(false);
        leaderboardScreen.SetActive(false);
        achievementScreen.SetActive(false);
    }

    private void ShowAchievementNotif(string achievementName)
    {
        achievementUIInGame.SetActive(true);
        achievementNameText.text = $"{achievementName}:";
        StartCoroutine(WaitAndTurnOff(achievementUIInGame, 1.2f));
    }

    private void SaveDataUIShow()
    {
        gameOverPanel.SetActive(false);
        gameSavingPanel.SetActive(true);
    }

    private IEnumerator WaitAndTurnOff(GameObject obj, float time = 0.3f, bool reset = false, int score = 0)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);

        if (reset) 
        {
            gameOverPanel.SetActive(true);
            yourScoreText.text = score.ToString("D10");
            bestScoreText.text = playerDataManager.BestScore.ToString("D10");
        }
    }
}
