using Core.Enums;
using Core.Events;
using Zenject;

namespace Services.PlayerInput
{
    public class InputService : IInitializable, ITickable
    {
        private readonly SignalBus _signalBus;
        private readonly IInputProvider _inputProvider;
        private bool _isActive = true;

        public InputService(SignalBus signalBus, [InjectOptional] IInputProvider inputProvider)
        {
            _signalBus = signalBus;
            _inputProvider = inputProvider;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GameStateChangedSignal>(OnStateChanged);
            _inputProvider?.SetActive(_isActive);
        }

        public void Tick()
        {
            if (!_isActive || _inputProvider == null) return;
            _inputProvider.ProcessInput();
        }

        private void OnStateChanged(GameStateChangedSignal signal)
        {
            _isActive = (signal.NewState == GameState.InGame);
            _inputProvider?.SetActive(_isActive);
        }
    }
}