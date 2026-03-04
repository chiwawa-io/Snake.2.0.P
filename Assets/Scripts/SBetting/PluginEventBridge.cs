using System;
using Core.Events;
using Luxodd.Game.HelpersAndUtils.Utils;
using Luxodd.Game.Scripts.HelpersAndUtils.Missions;
using Zenject;

namespace SBetting
{
    public class PluginEventBridge : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;

        public PluginEventBridge(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<LevelCompletedSignal>(OnLevelCompleted);
        }

        private void OnLevelCompleted()
        {
            EventAggregator.Post(this, new FinishLevelEvent());
        }
    }
}