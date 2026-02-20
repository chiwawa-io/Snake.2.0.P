using System;
using Core.Events;
using Zenject;

namespace Services.Gameloop
{
    public struct GameSessionStats
    {
        public int DistanceTravelled;
        public int GemsCollected;
        public int PreciousGemsCollected;
        public int TrapsAvoided;
        public int PowerUpsCollected;
    }

    public class StatsRecorder : IInitializable, IDisposable
    {


        private readonly SignalBus _signalBus;

        private int _distanceTravelled;
        private int _gemsCollected;
        private int _preciousGemsCollected;
        private int _trapsAvoided;
        private int _powerUpsCollected;

        public StatsRecorder(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GameStartedSignal>(ResetStats);

            _signalBus.Subscribe<PowerUpCollected>(PowerUpCollected);
            _signalBus.Subscribe<GemCollected>(GemsCollected);
            _signalBus.Subscribe<PreciousGemCollected>(PreciousGemCollected);
            _signalBus.Subscribe<TrapsAvoided>(TrapsAvoided);
            _signalBus.Subscribe<DistanceTravelled>(DistanceTravelled);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<GameStartedSignal>(ResetStats);

            _signalBus.Unsubscribe<PowerUpCollected>(PowerUpCollected);
            _signalBus.Unsubscribe<GemCollected>(GemsCollected);
            _signalBus.Unsubscribe<PreciousGemCollected>(PreciousGemCollected);
            _signalBus.Unsubscribe<TrapsAvoided>(TrapsAvoided);
            _signalBus.Unsubscribe<DistanceTravelled>(DistanceTravelled);
        }

        public GameSessionStats GetSessionStats()
        {
            return new GameSessionStats
            {
                DistanceTravelled = _distanceTravelled,
                GemsCollected = _gemsCollected,
                PowerUpsCollected = _powerUpsCollected,
                PreciousGemsCollected = _preciousGemsCollected,
                TrapsAvoided = _trapsAvoided
            };
        }

        private void ResetStats()
        {
            _distanceTravelled = 0;
            _gemsCollected = 0;
            _preciousGemsCollected = 0;
            _trapsAvoided = 0;
            _powerUpsCollected = 0;
        }

        private void PowerUpCollected() => _powerUpsCollected++;
        private void GemsCollected() => _gemsCollected++;
        private void PreciousGemCollected() => _preciousGemsCollected++;
        private void TrapsAvoided(TrapsAvoided signal) => _trapsAvoided = signal.TrapsAvoidedCount;
        private void DistanceTravelled(DistanceTravelled signal) => _distanceTravelled = signal.Distance;
    }
}