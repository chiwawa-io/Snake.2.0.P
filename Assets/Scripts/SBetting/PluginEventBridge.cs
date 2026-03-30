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
            _signalBus.Subscribe<LengthUpdatedSignal>(OnLengthUpdated);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<LevelCompletedSignal>(OnLevelCompleted);
            _signalBus.TryUnsubscribe<LengthUpdatedSignal>(OnLengthUpdated);
        }

        private void OnLevelCompleted()
        {
            EventAggregator.Post(this, new FinishLevelEvent());
        }

        private void OnLengthUpdated(LengthUpdatedSignal signal)
        {
            // Update the mission plugin with the new length
            _signalBus.Fire(new UpdateMissionProgressSignal(signal.CurrentLength));
        }
    }
}