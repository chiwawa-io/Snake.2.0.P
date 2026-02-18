using System.Collections.Generic;
using Core.Events;
using Core.Services; 
using Game.UI;
using Gameplay.Snake;
using Services.Backend;
using Services.PlayerData;
using UI.Achievements.Logic;
using UI.Achievements.Presenters;
using UI.Achievements.Views;
using UI.Game.Presenters;
using UI.Global;
using UI.MainMenu.Presenters;
using UI.MainMenu.Views;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [Header("--- Scene Systems ---")]
    [SerializeField] private SnakeView _snakeView;
    [SerializeField] private GameObject _gameElements;
    [SerializeField] private ItemSpawner _itemSpawner;
    [SerializeField] private NetworkManager _networkManager; 
    [SerializeField] private PlayerDataManager _playerDataManager;
    [SerializeField] private AudioManager _audioManager;
    
    [Header("--- UI Views ---")]
    [SerializeField] private MainMenuView _mainMenuView;
    [SerializeField] private GameView _hudView;
    [SerializeField] private AchievementsView _achievementsView;
    [SerializeField] private LeaderboardView _leaderboardView;
    [SerializeField] private BaseView _loadingView;
    [SerializeField] private ErrorView _errorView;

    [Header("--- Data & Config ---")]
    [SerializeField] private List<AchievementSO> _achievementList;
    [SerializeField] private AchievementCompletion _achievementConfig;
    [SerializeField] private Vector2Int _gridSize = new(22, 22);

    public override void InstallBindings()
    {
        InstallCoreSignals();
        InstallGameSystems();
        InstallUISystems();
        InstallAudio();
        
        Container.BindExecutionOrder<UIManager>(1);
        Container.BindExecutionOrder<Startup>(2);
    }

    private void InstallCoreSignals()
    {
        SignalBusInstaller.Install(Container);

        Container.DeclareSignal<GameStateChangedSignal>();
        Container.DeclareSignal<GameOverSignal>();
        Container.DeclareSignal<GameStartedSignal>();
        Container.DeclareSignal<RevivePlayerSignal>();

        Container.DeclareSignal<ScoreUpdatedSignal>();
        Container.DeclareSignal<ScoreAddedSignal>();
        Container.DeclareSignal<LifeUpdatedSignal>();
        
        Container.DeclareSignal<InputDirectionSignal>();
        Container.DeclareSignal<PlayerDiedSignal>();
        Container.DeclareSignal<PlaySoundSignal>();
        Container.DeclareSignal<ItemDestroyedSignal>();
        Container.DeclareSignal<PreciousGemEatenSignal>();
        Container.DeclareSignal<SnakeEffectSignal>(); 
        Container.DeclareSignal<ErrorSignal>();
        
        Container.DeclareSignal<InactivityTimerSignal>();
        Container.DeclareSignal<InactivityTimeOut>();
        
        Container.DeclareSignal<AchievementProgressSignal>();
    }

    private void InstallGameSystems()
    {
        Container.BindInterfacesAndSelfTo<Startup>().AsSingle();
        Container.Bind<PlayerDataManager>().FromInstance(_playerDataManager).AsSingle();
        Container.Bind<NetworkManager>().FromInstance(_networkManager).AsSingle();

        Container.BindInterfacesTo<InputService>().AsSingle();
        
        Container.BindInterfacesAndSelfTo<LuxoddBackendService>().AsSingle();

        Container.Bind<GameObject>().WithId("GameElements").FromInstance(_gameElements);
        Container.Bind<SnakeModel>().AsSingle();
        Container.Bind<SnakeEngine>().AsSingle().WithArguments(new Vector2(_gridSize.x, _gridSize.y));
        Container.Bind<AchievementCompletion>().FromInstance(_achievementConfig).AsSingle();
        
        Container.Bind<SnakeView>().FromInstance(_snakeView).AsSingle();
        Container.Bind<ItemSpawner>().FromInstance(_itemSpawner).AsSingle();

        Container.BindInterfacesAndSelfTo<SnakeController>().AsSingle();
        Container.BindInterfacesAndSelfTo<GameSessionController>().AsSingle();
    }

    private void InstallUISystems()
    {
        Container.Bind<MainMenuView>().FromInstance(_mainMenuView).AsSingle();
        Container.Bind<GameView>().FromInstance(_hudView).AsSingle();
        Container.Bind<AchievementsView>().FromInstance(_achievementsView).AsSingle();
        Container.Bind<LeaderboardView>().FromInstance(_leaderboardView).AsSingle();
        Container.Bind<ErrorView>().FromInstance(_errorView).AsSingle();
        Container.Bind<BaseView>().WithId("Loading").FromInstance(_loadingView);

        Container.Bind<AchievementService>().AsSingle().WithArguments(_achievementList);
        
        Container.BindInterfacesAndSelfTo<MainMenuPresenter>().AsSingle();
        Container.BindInterfacesAndSelfTo<AchievementsPresenter>().AsSingle();
        Container.BindInterfacesAndSelfTo<GamePresenter>().AsSingle();
        
        Container.BindInterfacesAndSelfTo<UIManager>().AsSingle().NonLazy();
    }

    private void InstallAudio()
    {
        if (_audioManager != null)
        {
            Container.Bind<AudioManager>()
                .FromInstance(_audioManager)
                .AsSingle();

            Container.Bind<IAudioService>()
                .To<AudioManager>()
                .FromResolve(); 
        }
        else
        {
            Debug.LogError("AudioManager is missing from the Installer reference slot!");
        }
    }
}