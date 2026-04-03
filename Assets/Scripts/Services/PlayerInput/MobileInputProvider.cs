using Core.Events;
using UnityEngine;
using Zenject;

namespace Services.PlayerInput
{
    public class MobileInputProvider : IInputProvider
    {
        private readonly SignalBus _signalBus;
        private bool _isActive = true;

        public MobileInputProvider(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void SetActive(bool isActive)
        {
            _isActive = isActive;
        }

        public void ProcessInput()
        {
            // Event-driven. Do nothing here to save CPU cycles.
        }

        public void HandleInput(Vector2Int direction)
        {
            if (!_isActive) return;
            _signalBus.Fire(new InputDirectionSignal(direction));
        }
    }
}
