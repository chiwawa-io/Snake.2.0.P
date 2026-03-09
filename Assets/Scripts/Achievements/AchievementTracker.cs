using System;
using System.Collections.Generic;
using Achievements.Data;
using Core.Events;
using Gameplay.Snake;
using Zenject;

namespace Achievements
{
    public class AchievementTracker : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly SnakeModel _model;
        private readonly AchievementCompletion _config;

        private bool _isSbMode;

        public AchievementTracker(SignalBus signalBus, SnakeModel model, AchievementCompletion config)
        {
            _signalBus = signalBus;
            _model = model;
            _config = config;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GemCollected>(OnGemCollected);
            _signalBus.Subscribe<PreciousGemCollected>(OnGemCollected);
            _signalBus.Subscribe<PowerUpCollected>(OnPowerUpCollected);
            _signalBus.Subscribe<LengthUpdatedSignal>(OnLengthUpdated);
        }
        public void Dispose()
        {
            _signalBus.TryUnsubscribe<GemCollected>(OnGemCollected);
            _signalBus.TryUnsubscribe<PreciousGemCollected>(OnGemCollected);
            _signalBus.TryUnsubscribe<PowerUpCollected>(OnPowerUpCollected);
            _signalBus.TryUnsubscribe<LengthUpdatedSignal>(OnLengthUpdated);
        }
        private void OnGemCollected() =>  Check(_config.FoodRules, _model.GemsCollected);
        private void OnPowerUpCollected() => Check(_config.SpeedPowerUpRules, _model.SpeedUpsCollected);
        private void OnLengthUpdated(LengthUpdatedSignal signal) => Check(_config.SnakeSizeRules, signal.CurrentLength);

        private void Check(List<AchievementCompletion.AchievementRule> rules, int val)
        {
            int index = rules.FindIndex(r => r.Threshold == val);

            if (index >= 0) 
            {
                var rule = rules[index];
                _signalBus.Fire(new AchievementProgressSignal(rule.AchievementId, rule.AchievementName));
            }
        }

    }
}