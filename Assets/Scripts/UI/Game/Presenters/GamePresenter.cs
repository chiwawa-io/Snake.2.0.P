using System;
using Core.Events;
using Zenject;

namespace  UI.Game.Presenters
{
    public class GamePresenter : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly GameView _view;

        public GamePresenter(SignalBus signalBus, GameView view)
        {
            _signalBus = signalBus;
            _view = view;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<ScoreUpdatedSignal>(OnScoreUpdated);
            _signalBus.Subscribe<ScoreAddedSignal>(OnScoreAdded);
            _signalBus.Subscribe<LifeUpdatedSignal>(OnLivesUpdated);
            _signalBus.Subscribe<AchievementProgressSignal>(OnAchievementUnlocked);
            _signalBus.Subscribe<SnakeEffectSignal>(OnEffectMessage);
            _signalBus.Subscribe<GameOverSignal>(OnGameOver);
            
            // Init default state
            _view.SetLives(3); 
            _view.SetScoreDisplay(0);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<ScoreUpdatedSignal>(OnScoreUpdated);
            _signalBus.Unsubscribe<ScoreAddedSignal>(OnScoreAdded);
            _signalBus.Unsubscribe<LifeUpdatedSignal>(OnLivesUpdated);
            _signalBus.Unsubscribe<AchievementProgressSignal>(OnAchievementUnlocked);
            _signalBus.Unsubscribe<SnakeEffectSignal>(OnEffectMessage);
            _signalBus.Unsubscribe<GameOverSignal>(OnGameOver);
        }

        // --- Signal Handlers ---

        private void OnScoreUpdated(ScoreUpdatedSignal signal)
        {
            _view.SetScoreDisplay(signal.TotalScore);
        }

        private void OnScoreAdded(ScoreAddedSignal signal)
        {
            // Logic: Format the string here, View just shows it.
            string msg = $"+{signal.Amount}";
            _view.ShowFloatingText(msg, signal.Position);
        }

        private void OnEffectMessage(SnakeEffectSignal signal)
        {
            _view.ShowFloatingText(signal.EffectName, signal.Position);
        }

        private void OnLivesUpdated(LifeUpdatedSignal signal)
        {
            _view.SetLives(signal.LifeRemaining);
        }

        private void OnAchievementUnlocked(AchievementProgressSignal signal)
        {
            _view.ShowAchievementToast(signal.AchievementId);
        }

        private void OnGameOver(GameOverSignal signal)
        {
            _view.HideFloatingTextImmediate();
        }
    }
}

