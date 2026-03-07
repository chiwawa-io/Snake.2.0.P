using System;
using Core.Events;
using UI.Game.Views;
using Zenject;

namespace UI.Game.Presenters
{
    public class GamePresenter : IInitializable, IDisposable
    {
        private const int DefaultLives = 3;
        private const int StartingLength = 4;
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
            _signalBus.Subscribe<LengthUpdatedSignal>(OnLengthUpdated);
            _signalBus.Subscribe<GrowthTimerUpdatedSignal>(OnGrowthTimerUpdated);
            _signalBus.Subscribe<StrategicBettingStartedSignal>(OnSBStarted);
            
            _view.SetLives(DefaultLives); 
            _view.SetScoreDisplay(0);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<ScoreUpdatedSignal>(OnScoreUpdated);
            _signalBus.TryUnsubscribe<ScoreAddedSignal>(OnScoreAdded);
            _signalBus.TryUnsubscribe<LifeUpdatedSignal>(OnLivesUpdated);
            _signalBus.TryUnsubscribe<AchievementProgressSignal>(OnAchievementUnlocked);
            _signalBus.TryUnsubscribe<SnakeEffectSignal>(OnEffectMessage);
            _signalBus.TryUnsubscribe<GameOverSignal>(OnGameOver);
            _signalBus.TryUnsubscribe<LengthUpdatedSignal>(OnLengthUpdated);
            _signalBus.TryUnsubscribe<GrowthTimerUpdatedSignal>(OnGrowthTimerUpdated);
        }

        private void OnSBStarted(StrategicBettingStartedSignal signal)
        {
            _view.SetScoreVisibility(false);
            _view.SetGrowthTimerVisibility(true);
            _view.SetLengthVisibility(true);
            _view.SetLength(StartingLength, signal.TargetLength);
            _view.SetGrowthTimer(10);
        }

        private void OnScoreUpdated(ScoreUpdatedSignal signal)
        {
            _view.SetScoreDisplay(signal.TotalScore);
        }

        private void OnScoreAdded(ScoreAddedSignal signal)
        {
            string msg = $"+{signal.Amount}";
            _view.ShowFloatingText(msg, signal.Position);
        }

        private void OnLengthUpdated(LengthUpdatedSignal signal)
        {
            _view.SetLength(signal.CurrentLength, signal.TargetLength);
        }

        private void OnGrowthTimerUpdated(GrowthTimerUpdatedSignal signal)
        {
            _view.SetGrowthTimer((int)signal.TimeRemaining);
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
            _view.ShowAchievementToast(signal.AchievementName);
        }

        private void OnGameOver(GameOverSignal signal)
        {
            _view.HideFloatingTextImmediate();
        }
    }
}