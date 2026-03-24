using System;
using Core.Enums;
using Core.Events;
using Zenject;

namespace UI.Game.Presenters
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
            _signalBus.Subscribe<StrategicBettingStartedSignal>(OnSbStarted);
            _signalBus.Subscribe<LengthUpdatedSignal>(OnLengthUpdated);
            _signalBus.Subscribe<GrowthTimerUpdatedSignal>(OnGrowthTimerUpdated);
            _signalBus.Subscribe<GameStateChangedSignal>(OnStateChanged);
            
            _view.SetLives(3); 
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
            _signalBus.TryUnsubscribe<StrategicBettingStartedSignal>(OnSbStarted);
            _signalBus.TryUnsubscribe<LengthUpdatedSignal>(OnLengthUpdated);
            _signalBus.TryUnsubscribe<GrowthTimerUpdatedSignal>(OnGrowthTimerUpdated);
            _signalBus.TryUnsubscribe<GameStateChangedSignal>(OnStateChanged);
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

        private void OnStateChanged(GameStateChangedSignal signal)
        {
            if (signal.NewState == GameState.MainMenu)
            {
                _view.ToggleSbUI();
            }
        }

        private void OnSbStarted(StrategicBettingStartedSignal signal)
        {
            _view.ToggleSbUI();
            _view.UpdateGoal(0, signal.TargetLength);
            
            _view.ShowBriefing(signal.TargetLength);
        }

        private void OnLengthUpdated(LengthUpdatedSignal signal)
        {
            _view.UpdateGoal(signal.CurrentLength, signal.TargetLength);
        }

        private void OnGrowthTimerUpdated(GrowthTimerUpdatedSignal signal)
        {
            var percent = signal.TimeRemaining / signal.TotalTime;
            var critical = percent < 0.25f;
            _view.UpdateTimer((int)signal.TimeRemaining, critical);
        }
    }
}