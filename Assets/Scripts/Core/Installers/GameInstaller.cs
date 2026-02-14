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
    [SerializeField] private SnakeView snakeView;
    [SerializeField] private GameObject gameElements;
    [SerializeField] private ItemSpawner itemSpawner;
    [SerializeField] private NetworkManager networkManager; 
    [SerializeField] private PlayerDataManager playerDataManager;
    [SerializeField] private AudioManager audioManager;

    [Header("--- UI Views ---")]
    [SerializeField] private MainMenuView mainMenuView;
    [SerializeField] private GameView hudView;
    [SerializeField] private AchievementsView achievementsView;
    [SerializeField] private LeaderboardView leaderboardView;
    [SerializeField] private BaseView loadingView;
    [SerializeField] private ErrorView errorView;

    [Header("--- Data & Config ---")]
    [SerializeField] private List<AchievementSO> achievementList;
    [SerializeField] private Vector2Int gridSize = new(22, 22);

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

        // Game State
        Container.DeclareSignal<GameStateChangedSignal>();
        Container.DeclareSignal<GameOverSignal>();
        Container.DeclareSignal<GameStartedSignal>();
        Container.DeclareSignal<RevivePlayerSignal>();

        // UI
        Container.DeclareSignal<ScoreUpdatedSignal>();
        Container.DeclareSignal<ScoreAddedSignal>();
        Container.DeclareSignal<LifeUpdatedSignal>();
        
        // Snake
        Container.DeclareSignal<InputDirectionSignal>();
        Container.DeclareSignal<PlayerDiedSignal>();
        Container.DeclareSignal<PlaySoundSignal>();
        Container.DeclareSignal<ItemDestroyedSignal>();
        Container.DeclareSignal<PreciousGemEatenSignal>();
        Container.DeclareSignal<SnakeEffectSignal>(); 
        Container.DeclareSignal<ErrorSignal>();
        
        // Timer
        Container.DeclareSignal<InactivityTimerSignal>();
        Container.DeclareSignal<InactivityTimeOut>();
        
        // Achievements
        Container.DeclareSignal<AchievementProgressSignal>();
    }

    private void InstallGameSystems()
    {
        Container.BindInterfacesAndSelfTo<Startup>().AsSingle();
        Container.Bind<PlayerDataManager>().FromInstance(playerDataManager).AsSingle();
        Container.Bind<NetworkManager>().FromInstance(networkManager).AsSingle();

        Container.BindInterfacesTo<InputService>().AsSingle();
        
        Container.BindInterfacesAndSelfTo<LuxoddBackendService>().AsSingle();

        Container.Bind<GameObject>().WithId("GameElements").FromInstance(gameElements);
        Container.Bind<SnakeModel>().AsSingle();
        Container.Bind<SnakeEngine>().AsSingle().WithArguments(new Vector2(gridSize.x, gridSize.y));
        
        Container.Bind<SnakeView>().FromInstance(snakeView).AsSingle();
        Container.Bind<ItemSpawner>().FromInstance(itemSpawner).AsSingle();

        Container.BindInterfacesAndSelfTo<SnakeGameController>().AsSingle();
        Container.BindInterfacesAndSelfTo<GameSessionController>().AsSingle();
    }

    private void InstallUISystems()
    {
        // Views
        Container.Bind<MainMenuView>().FromInstance(mainMenuView).AsSingle();
        Container.Bind<GameView>().FromInstance(hudView).AsSingle();
        Container.Bind<AchievementsView>().FromInstance(achievementsView).AsSingle();
        Container.Bind<LeaderboardView>().FromInstance(leaderboardView).AsSingle();
        Container.Bind<ErrorView>().FromInstance(errorView).AsSingle();
        Container.Bind<BaseView>().WithId("Loading").FromInstance(loadingView);

        // Logic
        Container.Bind<AchievementService>().AsSingle().WithArguments(achievementList);
        
        Container.BindInterfacesAndSelfTo<MainMenuPresenter>().AsSingle();
        Container.BindInterfacesAndSelfTo<AchievementsPresenter>().AsSingle();
        Container.BindInterfacesAndSelfTo<GamePresenter>().AsSingle();
        
        // Manager
        Container.BindInterfacesAndSelfTo<UIManager>().AsSingle().NonLazy();
    }

    private void InstallAudio()
    {
        if (audioManager != null)
        {
            Container.Bind<AudioManager>()
                .FromInstance(audioManager)
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