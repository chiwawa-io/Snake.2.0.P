using System;
using Core.Enums;
using Core.Events;
using DG.Tweening;
using Services.Gameloop;
using Zenject;
using UI.Game.Views;

namespace UI.Game.Presenters
{
    public class StatsPresenter : IInitializable, IDisposable
    {
        private const float DisplayDuration = 5.0f;

        private readonly StatsView _view;
        private readonly StatsRecorder _statsRecorder;
        private readonly SignalBus _signalBus;

        public StatsPresenter(
            StatsView view, 
            StatsRecorder statsRecorder, 
            SignalBus signalBus)
        {
            _view = view;
            _statsRecorder = statsRecorder;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GameStateChangedSignal>(OnStateChanged);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<GameStateChangedSignal>(OnStateChanged);
        }

        private void OnStateChanged(GameStateChangedSignal signal)
        {
            if (signal.NewState == GameState.PostGameStats)
            {
                ShowStatsSequence();
            }
        }

        private void ShowStatsSequence()
        {
            var stats = _statsRecorder.GetSessionStats();

            _view.DisplayStats(
                stats.GemsCollected, 
                stats.PreciousGemsCollected, 
                stats.PowerUpsCollected, 
                stats.DistanceTravelled, 
                stats.TrapsAvoided
            );

            _view.Show();

            DOVirtual.DelayedCall(DisplayDuration, () => 
            {
                _view.Hide();
                _signalBus.Fire(new GameStateChangedSignal(GameState.Leaderboard));
            });
        }
        
    }
}