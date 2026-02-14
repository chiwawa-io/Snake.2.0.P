using System;
using Core.Enums;
using Core.Events;   
using Services.Backend;
using Services.PlayerData;
using UI.MainMenu.Views;
using Zenject;

namespace UI.MainMenu.Presenters
{
    public class MainMenuPresenter : IInitializable, IDisposable
    {
        private readonly MainMenuView _view;
        private readonly PlayerDataManager _dataManager;
        private readonly SignalBus _signalBus;
        private readonly LuxoddBackendService _backendService;
        private readonly GameSessionController _sessionController; 

        public MainMenuPresenter(
            MainMenuView view, 
            PlayerDataManager dataManager, 
            SignalBus bus,
            GameSessionController sessionController,
            LuxoddBackendService backendService)
        {
            _view = view;
            _dataManager = dataManager;
            _signalBus = bus;
            _sessionController = sessionController;
            _backendService = backendService;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<InactivityTimerSignal>(OnTimerUpdate);
        }
        
        public void Dispose()
        {
            _signalBus.Unsubscribe<InactivityTimerSignal>(OnTimerUpdate);
        }

        public void OnShow()
        {
            int level = _dataManager.GetLevel();
            float xp = _dataManager.GetExpNormalized();
            _view.UpdatePlayerStats(level, xp);
        }

        public void OnPlayClicked()
        {
            _view.SetDifficultyPopupActive(true);
        }

        public void OnDifficultySelected(string difficulty)
        {
            _sessionController.SetDifficulty(difficulty);

            _signalBus.Fire(new GameStateChangedSignal { NewState = GameState.InGame });
        }

        public void OnLeaderboardClicked()
        {
            _signalBus.Fire(new GameStateChangedSignal { NewState = GameState.Leaderboard });
        }
        
        public void OnAchievementsClicked()
        {
            _signalBus.Fire(new GameStateChangedSignal { NewState = GameState.Achievements });
        }

        public void OnQuitClicked()
        {
            _backendService.Exit();
        }

        public void OnDifficultyClosed()
        {
            _view.SetDifficultyPopupActive(false);
        }

        private void OnTimerUpdate(InactivityTimerSignal signal)
        {
            _view.UpdateTimer(signal.SecondsLeft.ToString());
        }
    }
}