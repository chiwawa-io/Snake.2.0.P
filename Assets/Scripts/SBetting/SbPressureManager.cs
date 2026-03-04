using System;
using Core.Events;
using Gameplay.Snake;
using Services.Audio;
using UnityEngine;
using Zenject;

namespace SBetting
{
    public class SbPressureManager : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly SnakeModel _model;
        
        private bool _isActive;
        private float _growthTimer;
        private float _maxGrowthTime;
        
        private const float MaxPlayableFrequency = 0.05f;
        private const float PenaltyMultiplier = 0.05f;

        public SbPressureManager(SignalBus signalBus, SnakeModel model)
        {
            _signalBus = signalBus;
            _model = model;
        }

        public void Initialize() => _signalBus.Subscribe<StrategicBettingStartedSignal>(StartSb);
        public void Dispose() => _signalBus.TryUnsubscribe<StrategicBettingStartedSignal>(StartSb);

        public void Tick(float dt)
        {
            if (!_isActive) return;

            _growthTimer -= dt;
            _signalBus.Fire(new GrowthTimerUpdatedSignal(_growthTimer, _maxGrowthTime));

            if (_growthTimer <= 0) ApplySpeedPenalty();
        }

        public void ResetTimer() => _growthTimer = _maxGrowthTime;

        private void StartSb(StrategicBettingStartedSignal signal)
        {
            _isActive = true;
            _maxGrowthTime = Mathf.Lerp(15f, 4f, signal.Hardness / 100f);
            ResetTimer();
        }

        private void ApplySpeedPenalty()
        {
            float currentGap = _model.MoveFrequency - MaxPlayableFrequency;
            _model.MoveFrequency -= currentGap * PenaltyMultiplier;
            ResetTimer();
            
            _signalBus.Fire(new SnakeEffectSignal("SPEED UP!", _model.Body[0]));
            _signalBus.Fire(new PlaySoundSignal(SoundType.SpeedUp));
        }
    }
}