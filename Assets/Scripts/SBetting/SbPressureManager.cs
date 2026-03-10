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
        
        private float _maxGrowthTime = 10f;
        private bool _isActive;
        private float _growthTimer;
        
        private const float MaxPlayableFrequency = 0.05f;
        private const float PenaltyMultiplier = 0.5f;

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

        public void SetGrowthTimer(int timer)
        {
            _maxGrowthTime = timer;
        }

        private void StartSb(StrategicBettingStartedSignal signal)
        {
            _isActive = true;
            ResetTimer();
        }

        private void ApplySpeedPenalty()
        {
            Debug.LogWarning(_model.MoveFrequency);
            
            float currentGap = _model.BaseMoveFrequency - MaxPlayableFrequency;
            float reduction = currentGap * PenaltyMultiplier;
    
            _model.BaseMoveFrequency -= reduction;
            
            if (_model.MoveFrequency >= _model.BaseMoveFrequency)
            {
                _model.MoveFrequency = _model.BaseMoveFrequency;
            }
            
            Debug.LogWarning(_model.MoveFrequency);
            ResetTimer();
            
            _signalBus.Fire(new SnakeEffectSignal("SPEED UP!", _model.Body[0]));
            _signalBus.Fire(new PlaySoundSignal(SoundType.SpeedUp));
        }
    }
}