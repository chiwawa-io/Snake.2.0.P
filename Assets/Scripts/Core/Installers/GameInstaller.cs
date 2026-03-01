using System.Collections.Generic;
using Achievements.Data;
using Core.Events;
using Gameplay.Board;
using Gameplay.GameItem;
using Gameplay.Global;
using Gameplay.Global.Data;
using Gameplay.Snake;
using Gameplay.SpawnConfig;
using Services.Audio;
using Services.Backend;
using Services.Gameloop;
using Services.PlayerData;
using Services.PlayerInput;
using Services.RNG;
using UI.Achievements.Logic;
using UI.Achievements.Presenters;
using UI.Achievements.Views;
using UI.Game.Presenters;
using UI.Game.Views;
using UI.Global;
using UI.Leaderboard.Views;
using UI.MainMenu.Presenters;
using UI.MainMenu.Views;
using UI.Other;
using UnityEngine;
using Zenject;

namespace Core.Installers
{

    public class GameInstaller : MonoInstaller
    {
        [Header("--- Scene Systems ---")] 
        [SerializeField] private SnakeView _snakeView;
        [SerializeField] private BoardVisuals _boardVisuals;

        [SerializeField] private GameObject _gameElements;
        [SerializeField] private ItemSpawner _itemSpawner;
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private PlayerDataManager _playerDataManager;
        [SerializeField] private AudioManager _audioManager;

        [Header("--- UI Views ---")] 
        [SerializeField] private MainMenuView _mainMenuView;
        [SerializeField] private AchievementsView _achievementsView;
        [SerializeField] private LeaderboardView _leaderboardView;
        [SerializeField] private GameView _hudView;
        [SerializeField] private StatsView _statsView;
        [SerializeField] private BaseView _loadingView;
        [SerializeField] private ErrorView _errorView;

        [Header("--- Data & Config ---")] 
        [SerializeField] private List<AchievementSO> _achievementList;
        [SerializeField] private LevelBoardsConfig _levelBoardsConfig;
        [SerializeField] private AchievementCompletion _achievementConfig;

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

            Container.DeclareSignal<GemCollected>();
            Container.DeclareSignal<PowerUpCollected>();
            Container.DeclareSignal<PreciousGemCollected>();
            Container.DeclareSignal<TrapAvoided>();
            Container.DeclareSignal<DistanceTravelled>();

            Container.DeclareSignal<InputDirectionSignal>();
            Container.DeclareSignal<PlayerDiedSignal>();
            Container.DeclareSignal<PlaySoundSignal>();
            Container.DeclareSignal<ItemDestroyedSignal>();
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
            Container.BindInterfacesAndSelfTo<StatsRecorder>().AsSingle();

            Container.Bind<GameObject>().WithId("GameElements").FromInstance(_gameElements);
            Container.Bind<SnakeModel>().AsSingle();
            Container.Bind<SnakeEngine>().AsSingle();
            Container.Bind<SnakeView>().FromInstance(_snakeView).AsSingle();
            Container.Bind<BoardVisuals>().FromInstance(_boardVisuals).AsSingle();
            Container.Bind<ItemSpawner>().FromInstance(_itemSpawner).AsSingle();
            Container.Bind<AchievementCompletion>().FromInstance(_achievementConfig).AsSingle();

            Container.BindInterfacesAndSelfTo<SnakeController>().AsSingle();
            Container.BindInterfacesAndSelfTo<SpawnMapGenerator>().AsSingle();
            Container.BindInterfacesAndSelfTo<RngService>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameSessionController>().AsSingle();
        }

        private void InstallUISystems()
        {
            Container.Bind<MainMenuView>().FromInstance(_mainMenuView).AsSingle();
            Container.Bind<AchievementsView>().FromInstance(_achievementsView).AsSingle();
            Container.Bind<LeaderboardView>().FromInstance(_leaderboardView).AsSingle();
            Container.Bind<GameView>().FromInstance(_hudView).AsSingle();
            Container.Bind<StatsView>().FromInstance(_statsView).AsSingle();
            Container.Bind<ErrorView>().FromInstance(_errorView).AsSingle();
            Container.Bind<BaseView>().WithId("Loading").FromInstance(_loadingView);

            Container.Bind<AchievementService>().AsSingle().WithArguments(_achievementList);
            Container.Bind<LevelBoardsConfig>().FromInstance(_levelBoardsConfig);

            Container.BindInterfacesAndSelfTo<MainMenuPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<AchievementsPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<GamePresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<StatsPresenter>().AsSingle();

            Container.BindInterfacesAndSelfTo<UIManager>().AsSingle().NonLazy();
        }

        private void InstallAudio()
        {
            Container.Bind<AudioManager>()
                .FromInstance(_audioManager)
                .AsSingle();

            Container.Bind<IAudioService>()
                .To<AudioManager>()
                .FromResolve();
        }
    }
}