using System;
using Core.Enums;
using Core.Events;
using Game.UI;
using Services.PlayerData;
using Zenject;

namespace UI.MainMenu.Presenters
{
    public class MainMenuPresenter : IInitializable, IDisposable
    {
        private readonly MainMenuView _view;
        private readonly PlayerDataManager _dataManager;
        private readonly SignalBus _signalBus;

        public MainMenuPresenter(MainMenuView view, PlayerDataManager dataManager, SignalBus bus)
        {
            _view = view;
            _dataManager = dataManager;
            _signalBus = bus;
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
            _signalBus.Fire(new GameStateChangedSignal { NewState = GameState.InGame });
            // TODO: Difficulty pop up show
        }

        private void OnTimerUpdate(InactivityTimerSignal signal)
        {
            _view.UpdateTimer(signal.SecondsLeft.ToString());
        }
    }
}

