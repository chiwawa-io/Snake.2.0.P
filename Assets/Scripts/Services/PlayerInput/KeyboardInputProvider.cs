using Core.Events;
using UnityEngine;
using Zenject;

namespace Services.PlayerInput
{
    public class KeyboardInputProvider : IInputProvider
    {
        private readonly SignalBus _signalBus;

        public KeyboardInputProvider(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void SetActive(bool isActive)
        {
            // Handled natively by InputService active check before ProcessInput is called.
        }

        public void ProcessInput()
        {
            float h = UnityEngine.Input.GetAxisRaw("Horizontal");
            float v = UnityEngine.Input.GetAxisRaw("Vertical");

            Vector2Int dir = Vector2Int.zero;

            if (Mathf.Abs(h) > 0.1f)
            {
                dir = new Vector2Int(h > 0 ? 1 : -1, 0);
            }
            else if (Mathf.Abs(v) > 0.1f)
            {
                dir = new Vector2Int(0, v > 0 ? 1 : -1);
            }

            if (dir != Vector2Int.zero)
            {
                _signalBus.Fire(new InputDirectionSignal(dir));
            }
        }
    }
}
